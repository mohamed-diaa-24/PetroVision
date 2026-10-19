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
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CameraModelConfig> ModelConfigs { get; set; } = new List<CameraModelConfig>();
}
