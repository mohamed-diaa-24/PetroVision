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

public class ApproveSignupDto
{
    // Agents only; ignored for managers (they have every ability by role).
    public List<PermissionType> Permissions { get; set; } = new();
}
