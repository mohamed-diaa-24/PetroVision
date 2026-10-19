using System.ComponentModel.DataAnnotations;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.DTOs;

// Sent by the AI service
public class DetectionDto
{
    [Range(1, int.MaxValue)]
    public int CameraId { get; set; }

    [EnumDataType(typeof(DetectionType))]
    public DetectionType Type { get; set; }

    [Range(0, 1)]
    public double Confidence { get; set; }

    [StringLength(500)]
    public string? EvidenceUrl { get; set; }

    [StringLength(250)]
    public string? Reason { get; set; }
}

public class HeartbeatDto
{
    [Range(1, int.MaxValue)]
    public int CameraId { get; set; }

    public bool IsOnline { get; set; } = true;
}

public enum DetectionOutcome
{
    AlertCreated,
    IncidentCreated,
    IgnoredModelDisabled,
    IgnoredLowConfidence,
    IgnoredDuplicate
}

public class DetectionResultDto
{
    public DetectionOutcome Outcome { get; set; }
    public int? Id { get; set; }
}
