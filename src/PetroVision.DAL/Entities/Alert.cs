using PetroVision.DAL.Enums;

namespace PetroVision.DAL.Entities;

public class Alert
{
    public int Id { get; set; }
    public DetectionType Type { get; set; }
    public AlertLevel Level { get; set; }
    public AlertStatus Status { get; set; } = AlertStatus.New;
    public string Reason { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string? EvidenceUrl { get; set; }
    public int CameraId { get; set; }
    public int? HandledById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? HandledAt { get; set; }
    public DismissReason? DismissReason { get; set; }

    public Camera Camera { get; set; } = null!;
    public User? HandledBy { get; set; }
}
