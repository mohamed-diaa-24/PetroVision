using PetroVision.BLL.Interfaces;
using PetroVision.DAL.Context;
using PetroVision.DAL.Entities;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Services;

public class AuditLogger : IAuditLogger
{
    private const int DetailsMaxLength = 500;

    private readonly PetroVisionDbContext _db;
    private readonly ICurrentUser _user;

    public AuditLogger(PetroVisionDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public void Add(AuditAction action, string entityType, int? entityId, string item,
        string? details = null, int? departmentId = null, int? actorUserId = null)
    {
        if (details is { Length: > DetailsMaxLength })
            details = details[..(DetailsMaxLength - 1)] + "…";

        _db.AuditLogs.Add(new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Item = item.Length > 150 ? item[..150] : item,
            Details = details,
            UserId = actorUserId ?? (_user.UserId > 0 ? _user.UserId : null),
            DepartmentId = departmentId ?? _user.DepartmentId,
            CreatedAt = DateTime.UtcNow
        });
    }
}
