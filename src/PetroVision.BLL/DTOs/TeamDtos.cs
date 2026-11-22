using System.ComponentModel.DataAnnotations;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.DTOs;

public class SignupRequestItemDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int? DepartmentId { get; set; }
    public string? Department { get; set; }
    public DateTime AppliedAt { get; set; }
}

public class AgentDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PermissionType> Permissions { get; set; } = new();
}

public class CreateAgentDto
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    public List<PermissionType> Permissions { get; set; } = new();
}

public class CreatedAgentDto
{
    public AgentDto Agent { get; set; } = null!;

    // Shown once. The agent must change it at first login.
    public string TemporaryPassword { get; set; } = string.Empty;
}

public class UpdatePermissionsDto
{
    public List<PermissionType> Permissions { get; set; } = new();
}

public class ApproveSignupDto
{
    // Agents only; ignored for managers (they have every ability by role).
    public List<PermissionType> Permissions { get; set; } = new();
}
