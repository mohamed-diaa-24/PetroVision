using System.ComponentModel.DataAnnotations;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.DTOs;

public class CreateCameraRequestDto
{
    [EnumDataType(typeof(CameraRequestType))]
    public CameraRequestType Type { get; set; } = CameraRequestType.Add;

    // Required for Edit and Remove
    public int? CameraId { get; set; }

    // Add: Name and Location are required. Edit: empty = keep the current value.
    [StringLength(50)]
    public string? Name { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    [StringLength(500)]
    public string? StreamUrl { get; set; }

    // Add: models to enable. Edit: null = keep current models.
    public List<DetectionType>? Models { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}

public class RejectRequestDto
{
    [Required, StringLength(500, MinimumLength = 3)]
    public string Reason { get; set; } = string.Empty;
}

public class CameraValuesDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string StreamUrl { get; set; } = string.Empty;
    public List<DetectionType> Models { get; set; } = new();
}

public class CameraRequestDto
{
    public int Id { get; set; }
    public CameraRequestType Type { get; set; }
    public RequestStatus Status { get; set; }
    public int? CameraId { get; set; }
    public CameraValuesDto? Before { get; set; }   // current camera (Edit / Remove)
    public CameraValuesDto After { get; set; } = new();
    public string? Reason { get; set; }
    public int RequestedById { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNote { get; set; }
}
