using PetroVision.DAL.Enums;

namespace PetroVision.DAL.Entities;

public class Camera
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string StreamUrl { get; set; } = string.Empty;
    public CameraHealth Health { get; set; } = CameraHealth.Offline;
    public bool IsMuted { get; set; }

    // Soft delete: a removed camera keeps its alerts/incidents history.
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public ICollection<CameraModelConfig> ModelConfigs { get; set; } = new List<CameraModelConfig>();
}
