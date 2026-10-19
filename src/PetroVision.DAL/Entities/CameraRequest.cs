using PetroVision.DAL.Enums;

namespace PetroVision.DAL.Entities;

public class CameraRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string StreamUrl { get; set; } = string.Empty;
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public int RequestedById { get; set; }
    public int? ReviewedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public User RequestedBy { get; set; } = null!;
    public User? ReviewedBy { get; set; }
}
