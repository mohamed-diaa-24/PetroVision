using Microsoft.EntityFrameworkCore;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Exceptions;
using PetroVision.BLL.Interfaces;
using PetroVision.DAL.Context;
using PetroVision.DAL.Entities;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Services;

public class DetectionService : IDetectionService
{
    // PPE detections at or above this confidence become MEDIUM alerts, otherwise LOW.
    private const double MediumLevelConfidence = 0.75;

    private readonly PetroVisionDbContext _db;

    public DetectionService(PetroVisionDbContext db) => _db = db;

    public async Task<DetectionResultDto> ProcessAsync(DetectionDto dto)
    {
        var camera = await _db.Cameras
            .Include(c => c.ModelConfigs)
            .FirstOrDefaultAsync(c => c.Id == dto.CameraId)
            ?? throw new NotFoundException("Camera not found.");

        var now = DateTime.UtcNow;

        // Any detection proves the camera is alive.
        camera.Health = CameraHealth.Online;
        camera.LastSeenAt = now;

        // 1) Model must be assigned to this camera and enabled
        var config = camera.ModelConfigs.FirstOrDefault(m => m.Type == dto.Type);
        if (config is null || !config.IsEnabled)
            return await IgnoreAsync(DetectionOutcome.IgnoredModelDisabled);

        // 2) Confidence threshold
        if (dto.Confidence < config.ConfidenceThreshold)
            return await IgnoreAsync(DetectionOutcome.IgnoredLowConfidence);

        // 3) PPE -> Alert
        if (dto.Type == DetectionType.PPE)
        {
            if (await IsAlertCoolingDownAsync(camera.Id, config.CooldownMinutes, now))
                return await IgnoreAsync(DetectionOutcome.IgnoredDuplicate);

            var alert = new Alert
            {
                CameraId = camera.Id,
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

            return new DetectionResultDto { Outcome = DetectionOutcome.AlertCreated, Id = alert.Id };
        }

        // 4) Fall / Fire -> Incident
        var incidentType = dto.Type == DetectionType.Fall ? IncidentType.Fall : IncidentType.Fire;

        if (await IsIncidentDuplicateAsync(camera.Id, incidentType, config.CooldownMinutes, now))
            return await IgnoreAsync(DetectionOutcome.IgnoredDuplicate);

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

        return new DetectionResultDto { Outcome = DetectionOutcome.IncidentCreated, Id = incident.Id };
    }

    public async Task HeartbeatAsync(HeartbeatDto dto)
    {
        var camera = await _db.Cameras.FindAsync(dto.CameraId)
            ?? throw new NotFoundException("Camera not found.");

        camera.Health = dto.IsOnline ? CameraHealth.Online : CameraHealth.Offline;
        if (dto.IsOnline) camera.LastSeenAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

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
