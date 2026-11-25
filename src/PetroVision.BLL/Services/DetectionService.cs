using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Exceptions;
using PetroVision.BLL.Interfaces;
using PetroVision.BLL.Mapping;
using PetroVision.DAL.Context;
using PetroVision.DAL.Entities;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Services;

public class DetectionService : IDetectionService
{
    // PPE detections at or above this confidence become MEDIUM alerts, otherwise LOW.
    private const double MediumLevelConfidence = 0.75;

    private readonly PetroVisionDbContext _db;
    private readonly IEventNotifier _events;
    private readonly ILogger<DetectionService> _logger;

    public DetectionService(PetroVisionDbContext db, IEventNotifier events, ILogger<DetectionService> logger)
    {
        _db = db;
        _events = events;
        _logger = logger;
    }

    public async Task<DetectionResultDto> ProcessAsync(DetectionDto dto)
    {
        var camera = await _db.Cameras
            .Include(c => c.ModelConfigs)
            .FirstOrDefaultAsync(c => c.Id == dto.CameraId && !c.IsDeleted)
            ?? throw new NotFoundException("Camera not found.");

        var previousHealth = camera.Health;
        var now = DateTime.UtcNow;

        // Any detection proves the camera is alive.
        camera.Health = CameraHealth.Online;
        camera.LastSeenAt = now;

        var (result, alert, incident) = await ApplyAsync(camera, dto, now);

        // ---- real-time events (changes are saved at this point) ----
        if (previousHealth != camera.Health)
            await NotifyCameraStatusAsync(camera);

        if (alert is not null)
            await _events.TryNotifyAsync(_logger, "AlertCreated",
                n => n.AlertCreatedAsync(camera.DepartmentId, alert.ToDto()));

        if (incident is not null)
            await _events.TryNotifyAsync(_logger, "IncidentCreated", async n =>
            {
                var created = await _db.Incidents.AsNoTracking().WithDetails().FirstAsync(i => i.Id == incident.Id);
                await n.IncidentCreatedAsync(camera.DepartmentId, created.ToDto());
            });

        return result;
    }

    // Applies the detection rules and saves. Returns the created alert or incident, if any.
    private async Task<(DetectionResultDto Result, Alert? Alert, Incident? Incident)> ApplyAsync(
        Camera camera, DetectionDto dto, DateTime now)
    {
        // 1) Model must be assigned to this camera and enabled
        var config = camera.ModelConfigs.FirstOrDefault(m => m.Type == dto.Type);
        if (config is null || !config.IsEnabled)
            return (await IgnoreAsync(DetectionOutcome.IgnoredModelDisabled), null, null);

        // 2) Confidence threshold
        if (dto.Confidence < config.ConfidenceThreshold)
            return (await IgnoreAsync(DetectionOutcome.IgnoredLowConfidence), null, null);

        // 3) PPE -> Alert
        if (dto.Type == DetectionType.PPE)
        {
            if (await IsAlertCoolingDownAsync(camera.Id, config.CooldownMinutes, now))
                return (await IgnoreAsync(DetectionOutcome.IgnoredDuplicate), null, null);

            var alert = new Alert
            {
                CameraId = camera.Id,
                Camera = camera,
                Type = DetectionType.PPE,
                Level = dto.Confidence >= MediumLevelConfidence ? AlertLevel.Medium : AlertLevel.Low,
                Status = AlertStatus.New,
                Reason = string.IsNullOrWhiteSpace(dto.Reason) ? "Missing PPE" : dto.Reason.Trim(),
                Confidence = dto.Confidence,
                EvidenceUrl = dto.EvidenceUrl,
                CreatedAt = now
            };

            _db.Alerts.Add(alert);
            await _db.SaveChangesAsync();

            return (new DetectionResultDto { Outcome = DetectionOutcome.AlertCreated, Id = alert.Id }, alert, null);
        }

        // 4) Fall / Fire -> Incident
        var incidentType = dto.Type == DetectionType.Fall ? IncidentType.Fall : IncidentType.Fire;

        if (await IsIncidentDuplicateAsync(camera.Id, incidentType, config.CooldownMinutes, now))
            return (await IgnoreAsync(DetectionOutcome.IgnoredDuplicate), null, null);

        var incident = new Incident
        {
            CameraId = camera.Id,
            Type = incidentType,
            Status = IncidentStatus.Active,
            Confidence = dto.Confidence,
            EvidenceUrl = dto.EvidenceUrl,
            CreatedAt = now
        };

        _db.Incidents.Add(incident);
        await _db.SaveChangesAsync();

        return (new DetectionResultDto { Outcome = DetectionOutcome.IncidentCreated, Id = incident.Id }, null, incident);
    }

    public async Task HeartbeatAsync(HeartbeatDto dto)
    {
        var camera = await _db.Cameras.FirstOrDefaultAsync(c => c.Id == dto.CameraId && !c.IsDeleted)
            ?? throw new NotFoundException("Camera not found.");

        var previousHealth = camera.Health;
        camera.Health = dto.IsOnline ? CameraHealth.Online : CameraHealth.Offline;
        if (dto.IsOnline) camera.LastSeenAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Only real transitions (e.g. Offline -> Online), not every heartbeat.
        if (previousHealth != camera.Health)
            await NotifyCameraStatusAsync(camera);
    }

    private Task NotifyCameraStatusAsync(Camera camera)
        => _events.TryNotifyAsync(_logger, "CameraStatusChanged",
            n => n.CameraStatusChangedAsync(camera.DepartmentId, camera.Id, camera.Health, camera.LastSeenAt));

    private async Task<DetectionResultDto> IgnoreAsync(DetectionOutcome outcome)
    {
        await _db.SaveChangesAsync(); // keeps the camera heartbeat
        return new DetectionResultDto { Outcome = outcome };
    }

    private Task<bool> IsAlertCoolingDownAsync(int cameraId, int cooldownMinutes, DateTime now)
    {
        if (cooldownMinutes <= 0) return Task.FromResult(false);

        var cutoff = now.AddMinutes(-cooldownMinutes);
        return _db.Alerts.AnyAsync(a => a.CameraId == cameraId
                                        && a.Type == DetectionType.PPE
                                        && a.CreatedAt >= cutoff);
    }

    // No new incident while one of the same type is still open on that camera, or inside the cooldown.
    private Task<bool> IsIncidentDuplicateAsync(int cameraId, IncidentType type, int cooldownMinutes, DateTime now)
    {
        var cutoff = now.AddMinutes(-Math.Max(cooldownMinutes, 0));

        return _db.Incidents.AnyAsync(i => i.CameraId == cameraId
                                           && i.Type == type
                                           && (i.Status != IncidentStatus.Closed || i.CreatedAt >= cutoff));
    }
}
