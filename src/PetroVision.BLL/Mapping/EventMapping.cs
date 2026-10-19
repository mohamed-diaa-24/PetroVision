using Microsoft.EntityFrameworkCore;
using PetroVision.BLL.DTOs;
using PetroVision.DAL.Entities;

namespace PetroVision.BLL.Mapping;

public static class EventMapping
{
    public static IQueryable<Alert> WithDetails(this IQueryable<Alert> query)
        => query.Include(a => a.Camera).Include(a => a.HandledBy);

    public static IQueryable<Incident> WithDetails(this IQueryable<Incident> query)
        => query.Include(i => i.Camera).ThenInclude(c => c.Department).Include(i => i.Handler);

    public static AlertDto ToDto(this Alert a) => new()
    {
        Id = a.Id,
        Type = a.Type,
        Level = a.Level,
        Status = a.Status,
        Reason = a.Reason,
        Confidence = a.Confidence,
        EvidenceUrl = a.EvidenceUrl,
        CameraId = a.CameraId,
        CameraName = a.Camera.Name,
        Location = a.Camera.Location,
        CreatedAt = a.CreatedAt,
        HandledBy = a.HandledBy?.FullName,
        HandledAt = a.HandledAt,
        DismissReason = a.DismissReason
    };

    public static IncidentDto ToDto(this Incident i) => new()
    {
        Id = i.Id,
        Type = i.Type,
        Status = i.Status,
        Confidence = i.Confidence,
        EvidenceUrl = i.EvidenceUrl,
        Note = i.Note,
        SourceAlertId = i.SourceAlertId,
        CameraId = i.CameraId,
        CameraName = i.Camera.Name,
        Location = i.Camera.Location,
        Department = i.Camera.Department.Name,
        HandlerId = i.HandlerId,
        Handler = i.Handler?.FullName,
        CreatedAt = i.CreatedAt,
        ClosedAt = i.ClosedAt
    };
}
