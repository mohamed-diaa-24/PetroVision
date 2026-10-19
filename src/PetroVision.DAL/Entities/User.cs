using PetroVision.DAL.Enums;

namespace PetroVision.DAL.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActiveAt { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<AgentPermission> Permissions { get; set; } = new List<AgentPermission>();
}
