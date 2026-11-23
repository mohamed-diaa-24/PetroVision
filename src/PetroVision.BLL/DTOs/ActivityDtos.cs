using PetroVision.DAL.Enums;

namespace PetroVision.BLL.DTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public DateTime Time { get; set; }
    public int? UserId { get; set; }
    public string User { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string Item { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? Department { get; set; }
}

public class ActivityQuery
{
    public AuditAction? Action { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? UserId { get; set; }          // team view only
    public int? DepartmentId { get; set; }    // owner only
    public int Take { get; set; } = 100;
}
