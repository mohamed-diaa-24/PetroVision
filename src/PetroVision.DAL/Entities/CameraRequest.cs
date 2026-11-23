using PetroVision.DAL.Enums;

namespace PetroVision.DAL.Entities;

// An agent asks the department manager to add, edit or remove a camera.
public class CameraRequest
{
    public int Id { get; set; }
    public CameraRequestType Type { get; set; } = CameraRequestType.Add;
    public int? CameraId { get; set; }                      // Edit / Remove
    public int DepartmentId { get; set; }

    // Requested values (for Remove: a copy of the camera at request time)
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string StreamUrl { get; set; } = string.Empty;
    public string? Models { get; set; }                     // "PPE,Fall"; null on Edit = keep current models

    public string? Reason { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public string? ReviewNote { get; set; }                 // rejection reason
    public int RequestedById { get; set; }
    public int? ReviewedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public Camera? Camera { get; set; }
    public Department Department { get; set; } = null!;
    public User RequestedBy { get; set; } = null!;
    public User? ReviewedBy { get; set; }
}
