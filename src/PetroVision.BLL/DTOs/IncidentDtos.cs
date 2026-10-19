using System.ComponentModel.DataAnnotations;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.DTOs;

public class IncidentDto
{
    public int Id { get; set; }
    public IncidentType Type { get; set; }
    public IncidentStatus Status { get; set; }
    public double Confidence { get; set; }
    public string? EvidenceUrl { get; set; }
    public string? Note { get; set; }
    public int? SourceAlertId { get; set; }
    public int CameraId { get; set; }
    public string CameraName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int? HandlerId { get; set; }
    public string? Handler { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}

public class CreateIncidentDto
{
    [Range(1, int.MaxValue)]
    public int CameraId { get; set; }

    [EnumDataType(typeof(IncidentType))]
    public IncidentType Type { get; set; }

    [StringLength(1000)]
    public string? Note { get; set; }
}

public class CloseIncidentDto
{
    [Required, StringLength(1000)]
    public string Note { get; set; } = string.Empty;
}

public class ReassignIncidentDto
{
    [Range(1, int.MaxValue)]
    public int AssigneeId { get; set; }
}

public class AssigneeDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
