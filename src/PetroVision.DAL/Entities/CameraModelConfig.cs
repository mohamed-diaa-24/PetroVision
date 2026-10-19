using PetroVision.DAL.Enums;

namespace PetroVision.DAL.Entities;

public class CameraModelConfig
{
    public int Id { get; set; }
    public int CameraId { get; set; }
    public DetectionType Type { get; set; }
    public bool IsEnabled { get; set; } = true;
    public double ConfidenceThreshold { get; set; } = 0.70;
    public int CooldownMinutes { get; set; } = 2;
    public int EvidenceRetentionDays { get; set; } = 30;

    public Camera Camera { get; set; } = null!;
}
