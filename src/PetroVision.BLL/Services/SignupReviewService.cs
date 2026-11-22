using Microsoft.EntityFrameworkCore;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Exceptions;
using PetroVision.BLL.Interfaces;
using PetroVision.DAL.Context;
using PetroVision.DAL.Entities;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Services;

public class SignupReviewService : ISignupReviewService
{
    private readonly PetroVisionDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IAuditLogger _audit;

    public SignupReviewService(PetroVisionDbContext db, ICurrentUser user, IAuditLogger audit)
    {
        _db = db;
        _user = user;
        _audit = audit;
    }

    // The pending users this reviewer is allowed to decide on.
    private IQueryable<User> Reviewable()
    {
        var pending = _db.Users.Where(u => u.Status == UserStatus.Pending);

        if (_user.Role == "Owner")
            return pending.Where(u => u.Role == UserRole.Manager);

        if (_user.Role == "Manager")
        {
            var departmentId = _user.DepartmentId
                ?? throw new ForbiddenException("Your account is not assigned to a department yet.");
            return pending.Where(u => u.Role == UserRole.Agent && u.DepartmentId == departmentId);
        }

        throw new ForbiddenException("Only managers and the owner review sign-up requests.");
    }

    public async Task<List<SignupRequestItemDto>> GetPendingAsync()
    {
        var users = await Reviewable().AsNoTracking()
            .Include(u => u.Department)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

        return users.Select(ToDto).ToList();
    }

    public async Task<SignupRequestItemDto> ApproveAsync(int userId, List<PermissionType> permissions)
    {
        var user = await LoadAsync(userId);

        user.Status = UserStatus.Active;
        user.StatusReason = null;
        user.SecurityStamp = Guid.NewGuid().ToString("N");

        if (user.Role == UserRole.Agent)
        {
            _db.AgentPermissions.RemoveRange(user.Permissions);
            foreach (var p in permissions.Distinct())
                user.Permissions.Add(new AgentPermission { Permission = p });
        }

        _audit.Add(AuditAction.SignupApproved, nameof(User), user.Id, $"{user.FullName} ({user.EmployeeCode})",
            user.Role == UserRole.Agent && permissions.Count > 0
                ? "Permissions: " + string.Join(", ", permissions.Distinct())
                : $"Role: {user.Role}",
            user.DepartmentId);
        await _db.SaveChangesAsync();

        return ToDto(user);
    }

    public async Task<SignupRequestItemDto> RejectAsync(int userId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new BadRequestException("A rejection reason is required.");

        var user = await LoadAsync(userId);

        user.Status = UserStatus.Rejected;
        user.StatusReason = reason.Trim();

        _audit.Add(AuditAction.SignupRejected, nameof(User), user.Id, $"{user.FullName} ({user.EmployeeCode})",
            user.StatusReason, user.DepartmentId);
        await _db.SaveChangesAsync();

        return ToDto(user);
    }

    private async Task<User> LoadAsync(int userId)
        => await Reviewable()
               .Include(u => u.Department)
               .Include(u => u.Permissions)
               .FirstOrDefaultAsync(u => u.Id == userId)
           ?? throw new NotFoundException("Sign-up request not found.");

    private static SignupRequestItemDto ToDto(User u) => new()
    {
        UserId = u.Id,
        FullName = u.FullName,
        EmployeeCode = u.EmployeeCode,
        Email = u.Email,
        Role = u.Role,
        DepartmentId = u.DepartmentId,
        Department = u.Department?.Name,
        AppliedAt = u.CreatedAt
    };
}
