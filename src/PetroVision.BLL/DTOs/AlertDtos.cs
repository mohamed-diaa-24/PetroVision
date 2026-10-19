using System.ComponentModel.DataAnnotations;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.DTOs;

public class AlertDto
{
    public int Id { get; set; }
    public DetectionType Type { get; set; }
    public AlertLevel Level { get; set; }
    public AlertStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string? EvidenceUrl { get; set; }
    public int CameraId { get; set; }
    public string CameraName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? HandledBy { get; set; }
    public DateTime? HandledAt { get; set; }
    public DismissReason? DismissReason { get; set; }
}

public class DismissAlertDto
{
    [EnumDataType(typeof(DismissReason))]
    public DismissReason Reason { get; set; }
}
