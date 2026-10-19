using PetroVision.DAL.Enums;

namespace PetroVision.DAL.Entities;

public class Incident
{
    public int Id { get; set; }
    public DetectionType Type { get; set; }
    public IncidentStatus Status { get; set; } = IncidentStatus.Active;
    public double Confidence { get; set; }
    public string? EvidenceUrl { get; set; }
    public string? Note { get; set; }
    public int CameraId { get; set; }
    public int? HandlerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public Camera Camera { get; set; } = null!;
    public User? Handler { get; set; }
}
