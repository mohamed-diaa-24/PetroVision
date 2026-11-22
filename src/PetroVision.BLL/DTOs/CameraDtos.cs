using System.ComponentModel.DataAnnotations;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.DTOs;

public class CameraModelDto
{
    public DetectionType Type { get; set; }
    public bool IsEnabled { get; set; } = true;

    [Range(0.1, 1)]
    public double ConfidenceThreshold { get; set; } = 0.70;

    [Range(0, 1440)]
    public int CooldownMinutes { get; set; } = 2;

    [Range(1, 365)]
    public int EvidenceRetentionDays { get; set; } = 30;
}

public class CameraDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string StreamUrl { get; set; } = string.Empty;
    public CameraHealth Health { get; set; }
    public bool IsMuted { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public List<CameraModelDto> Models { get; set; } = new();
}

public class CreateCameraDto
{
    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [StringLength(500)]
    public string StreamUrl { get; set; } = string.Empty;

    public List<CameraModelDto> Models { get; set; } = new();
}

public class UpdateCameraDto
{
    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [StringLength(500)]
    public string StreamUrl { get; set; } = string.Empty;
}

public class MuteCameraDto
{
    public bool IsMuted { get; set; }
}

public class TestConnectionDto
{
    [Required, StringLength(500)]
    public string StreamUrl { get; set; } = string.Empty;
}

public class ConnectionTestResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long? LatencyMs { get; set; }
}
