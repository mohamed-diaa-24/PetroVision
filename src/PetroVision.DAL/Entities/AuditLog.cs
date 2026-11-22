using PetroVision.DAL.Enums;

namespace PetroVision.DAL.Entities;

// Who did what, on which item, and when. Written by the BLL, never edited.
public class AuditLog
{
    public int Id { get; set; }
    public AuditAction Action { get; set; }
    public string EntityType { get; set; } = string.Empty;   // "Alert", "Incident", "Camera", "User", ...
    public int? EntityId { get; set; }
    public string Item { get; set; } = string.Empty;         // display text, e.g. "Incident #1042"
    public string? Details { get; set; }
    public int? UserId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Department? Department { get; set; }
}
