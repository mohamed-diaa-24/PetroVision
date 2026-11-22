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
    public string? StatusReason { get; set; }   // e.g. why a sign-up request was rejected
    public bool MustChangePassword { get; set; } // set for accounts created with a temporary password
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActiveAt { get; set; }

    // Changes whenever access must be re-checked (permissions, status, password).
    // Tokens carry the stamp; a token with an old stamp is rejected.
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<AgentPermission> Permissions { get; set; } = new List<AgentPermission>();
}
