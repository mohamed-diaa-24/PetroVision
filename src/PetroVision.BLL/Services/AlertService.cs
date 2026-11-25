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

public class AlertService : IAlertService
{
    private readonly PetroVisionDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IAuditLogger _audit;
    private readonly IEventNotifier _events;
    private readonly ILogger<AlertService> _logger;

    public AlertService(PetroVisionDbContext db, ICurrentUser user, IAuditLogger audit,
        IEventNotifier events, ILogger<AlertService> logger)
    {
        _db = db;
        _user = user;
        _audit = audit;
        _events = events;
        _logger = logger;
    }

    private int DepartmentId => _user.DepartmentId
        ?? throw new ForbiddenException("Your account is not assigned to a department yet.");

    private IQueryable<Alert> Scoped()
    {
        var departmentId = DepartmentId;
        return _db.Alerts.Where(a => a.Camera.DepartmentId == departmentId);
    }

    private async Task<Alert> LoadAsync(int id)
        => await Scoped().FirstOrDefaultAsync(a => a.Id == id)
           ?? throw new NotFoundException("Alert not found.");

    public async Task<List<AlertDto>> GetAllAsync(AlertStatus? status, AlertLevel? level, int? cameraId)
    {
        IQueryable<Alert> query = Scoped().AsNoTracking().WithDetails();

        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        if (level.HasValue) query = query.Where(a => a.Level == level.Value);
        if (cameraId.HasValue) query = query.Where(a => a.CameraId == cameraId.Value);

        var alerts = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        return alerts.Select(a => a.ToDto()).ToList();
    }

    public async Task<AlertDto> GetByIdAsync(int id)
    {
        var alert = await Scoped().AsNoTracking().WithDetails().FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException("Alert not found.");

        return alert.ToDto();
    }

    public async Task<AlertDto> AcknowledgeAsync(int id)
    {
        var alert = await LoadAsync(id);

        if (alert.Status != AlertStatus.New)
            throw new ConflictException("Only new alerts can be acknowledged.");

        alert.Status = AlertStatus.Acknowledged;
        alert.HandledById = _user.UserId;
        alert.HandledAt = DateTime.UtcNow;

        _audit.Add(AuditAction.AlertAcknowledged, nameof(Alert), alert.Id, $"Alert #{alert.Id}");
        await _db.SaveChangesAsync();

        var result = await GetByIdAsync(id);
        await NotifyAlertUpdatedAsync(result);
        return result;
    }

    public async Task<AlertDto> DismissAsync(int id, DismissReason reason)
    {
        var alert = await LoadAsync(id);

        if (alert.Status == AlertStatus.Dismissed)
            throw new ConflictException("This alert is already dismissed.");

        alert.Status = AlertStatus.Dismissed;
        alert.DismissReason = reason;
        alert.HandledById = _user.UserId;
        alert.HandledAt = DateTime.UtcNow;

        _audit.Add(AuditAction.AlertDismissed, nameof(Alert), alert.Id, $"Alert #{alert.Id}", $"Reason: {reason}");
        await _db.SaveChangesAsync();

        var result = await GetByIdAsync(id);
        await NotifyAlertUpdatedAsync(result);
        return result;
    }

    public async Task<IncidentDto> EscalateAsync(int id)
    {
        var alert = await LoadAsync(id);

        if (alert.Status == AlertStatus.Dismissed)
            throw new ConflictException("Dismissed alerts cannot be escalated.");

        if (await _db.Incidents.AnyAsync(i => i.SourceAlertId == alert.Id))
            throw new ConflictException("This alert was already escalated to an incident.");

        var incident = new Incident
        {
            CameraId = alert.CameraId,
            Type = alert.Type switch
            {
                DetectionType.Fall => IncidentType.Fall,
                DetectionType.Fire => IncidentType.Fire,
                _ => IncidentType.Other
            },
            Status = IncidentStatus.Active,
            Confidence = alert.Confidence,
            EvidenceUrl = alert.EvidenceUrl,
            Note = $"Escalated from alert #{alert.Id}",
            SourceAlertId = alert.Id
        };

        var alertStatusChanged = alert.Status == AlertStatus.New;
        if (alertStatusChanged)
        {
            alert.Status = AlertStatus.Acknowledged;
            alert.HandledById = _user.UserId;
            alert.HandledAt = DateTime.UtcNow;
        }

        _db.Incidents.Add(incident);
        await _db.SaveChangesAsync();

        _audit.Add(AuditAction.AlertEscalated, nameof(Alert), alert.Id, $"Alert #{alert.Id}", $"Escalated to incident #{incident.Id}");
        await _db.SaveChangesAsync();

        var created = (await _db.Incidents.AsNoTracking().WithDetails().FirstAsync(i => i.Id == incident.Id)).ToDto();

        await _events.TryNotifyAsync(_logger, "IncidentCreated", n => n.IncidentCreatedAsync(DepartmentId, created));
        if (alertStatusChanged)
            await _events.TryNotifyAsync(_logger, "AlertUpdated", async n => await n.AlertUpdatedAsync(DepartmentId, await GetByIdAsync(id)));

        return created;
    }

    private Task NotifyAlertUpdatedAsync(AlertDto alert)
        => _events.TryNotifyAsync(_logger, "AlertUpdated", n => n.AlertUpdatedAsync(DepartmentId, alert));
}
