using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Interfaces;

// Adds an entry to the current DbContext; it is saved by the caller's next SaveChangesAsync,
// so the log entry and the change it describes are written together.
public interface IAuditLogger
{
    // actorUserId: who did it, when there is no signed-in user (e.g. password reset).
    void Add(AuditAction action, string entityType, int? entityId, string item,
        string? details = null, int? departmentId = null, int? actorUserId = null);
}
