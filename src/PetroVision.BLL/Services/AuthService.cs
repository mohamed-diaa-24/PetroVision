using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Exceptions;
using PetroVision.BLL.Interfaces;
using PetroVision.DAL.Context;
using PetroVision.DAL.Entities;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Services;

public class AuthService : IAuthService
{
    private static readonly TimeSpan ResetCodeLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ResetCodeResendDelay = TimeSpan.FromMinutes(1);
    private const int MaxResetAttempts = 5;

    private readonly PetroVisionDbContext _db;
    private readonly ITokenService _tokens;
    private readonly IAuditLogger _audit;
    private readonly IEmailSender _email;

    public AuthService(PetroVisionDbContext db, ITokenService tokens, IAuditLogger audit, IEmailSender email)
    {
        _db = db;
        _tokens = tokens;
        _audit = audit;
        _email = email;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await FindByIdentifierAsync(request.Identifier);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        switch (user.Status)
        {
            case UserStatus.Pending:
                throw new ForbiddenException(user.Role == UserRole.Manager
                    ? "Your account is awaiting the owner's approval."
                    : "Your account is awaiting your manager's approval.");
            case UserStatus.Rejected:
                throw new ForbiddenException(string.IsNullOrEmpty(user.StatusReason)
                    ? "Your sign-up request was rejected."
                    : $"Your sign-up request was rejected: {user.StatusReason}");
            case UserStatus.Expired:
                throw new ForbiddenException("Your account has expired. Please contact your manager.");
        }

        user.LastActiveAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return CreateLoginResponse(user);
    }

    public async Task<MessageResponseDto> SignupAsync(SignupRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var code = request.EmployeeCode.Trim();

        if (request.Role == UserRole.Owner)
            throw new BadRequestException("Owner accounts cannot be requested through sign-up.");

        if (!await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId))
            throw new BadRequestException("Department not found.");

        if (await _db.Users.AnyAsync(u => u.Email == email))
            throw new ConflictException("This email is already registered.");

        if (await _db.Users.AnyAsync(u => u.EmployeeCode == code))
            throw new ConflictException("This employee code is already registered.");

        _db.Users.Add(new User
        {
            FullName = request.FullName.Trim(),
            EmployeeCode = code,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            Status = UserStatus.Pending,
            DepartmentId = request.DepartmentId
        });

        await _db.SaveChangesAsync();

        return new MessageResponseDto
        {
            Message = request.Role == UserRole.Manager
                ? "Sign-up request submitted. Waiting for the owner's approval."
                : "Sign-up request submitted. Waiting for your department manager's approval."
        };
    }

    public async Task<UserDto> GetProfileAsync(int userId)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.Permissions)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User not found.");

        return ToDto(user);
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await LoadUserAsync(userId);
        var email = dto.Email.Trim().ToLowerInvariant();

        if (email != user.Email && await _db.Users.AnyAsync(u => u.Id != userId && u.Email == email))
            throw new ConflictException("This email is already registered.");

        user.FullName = dto.FullName.Trim();
        user.Email = email;

        _audit.Add(AuditAction.ProfileUpdated, nameof(User), user.Id, user.FullName);
        await _db.SaveChangesAsync();

        return ToDto(user);
    }

    // Returns a fresh token: the password change rotates the security stamp,
    // which signs out every other session.
    public async Task<LoginResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await LoadUserAsync(userId);

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new BadRequestException("Current password is incorrect.");

        if (dto.CurrentPassword == dto.NewPassword)
            throw new BadRequestException("The new password must be different from the current one.");

        SetPassword(user, dto.NewPassword);

        _audit.Add(AuditAction.PasswordChanged, nameof(User), user.Id, user.FullName);
        await _db.SaveChangesAsync();

        return CreateLoginResponse(user);
    }

    // Always answers the same way, so it cannot be used to find out which accounts exist.
    public async Task<MessageResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var response = new MessageResponseDto
        {
            Message = "If the account exists, a 6-digit code was sent to its email. It is valid for 15 minutes."
        };

        var user = await FindByIdentifierAsync(dto.Identifier);
        if (user is null || user.Status != UserStatus.Active) return response;

        var now = DateTime.UtcNow;
        var issuedAt = user.PasswordResetExpiresAt - ResetCodeLifetime;
        if (issuedAt.HasValue && now - issuedAt.Value < ResetCodeResendDelay) return response;

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        user.PasswordResetCodeHash = BCrypt.Net.BCrypt.HashPassword(code);
        user.PasswordResetExpiresAt = now + ResetCodeLifetime;
        user.PasswordResetAttempts = 0;
        await _db.SaveChangesAsync();

        await _email.SendAsync(user.Email, "PetroVision 360 password reset code",
            $"Hello {user.FullName},\n\nYour password reset code is {code}. It expires in 15 minutes.\n" +
            "If you did not ask for it, you can ignore this email.");

        return response;
    }

    public async Task<MessageResponseDto> ResetPasswordAsync(ResetPasswordDto dto)
    {
        const string invalid = "Invalid or expired code.";

        var user = await FindByIdentifierAsync(dto.Identifier);
        if (user is null || user.Status != UserStatus.Active
            || user.PasswordResetCodeHash is null || user.PasswordResetExpiresAt < DateTime.UtcNow)
            throw new BadRequestException(invalid);

        if (!BCrypt.Net.BCrypt.Verify(dto.Code, user.PasswordResetCodeHash))
        {
            user.PasswordResetAttempts++;
            if (user.PasswordResetAttempts >= MaxResetAttempts) ClearResetCode(user);   // ask for a new code
            await _db.SaveChangesAsync();
            throw new BadRequestException(invalid);
        }

        SetPassword(user, dto.NewPassword);

        _audit.Add(AuditAction.PasswordReset, nameof(User), user.Id, user.FullName,
            departmentId: user.DepartmentId, actorUserId: user.Id);
        await _db.SaveChangesAsync();

        return new MessageResponseDto { Message = "Password changed. You can log in with the new password." };
    }

    // ---------- helpers ----------
    private async Task<User?> FindByIdentifierAsync(string identifier)
    {
        identifier = identifier.Trim();
        var query = _db.Users.Include(u => u.Permissions).Include(u => u.Department);

        return identifier.Contains('@')
            ? await query.FirstOrDefaultAsync(u => u.Email == identifier.ToLowerInvariant())
            : await query.FirstOrDefaultAsync(u => u.EmployeeCode == identifier);
    }

    private async Task<User> LoadUserAsync(int userId)
        => await _db.Users
               .Include(u => u.Permissions)
               .Include(u => u.Department)
               .FirstOrDefaultAsync(u => u.Id == userId)
           ?? throw new NotFoundException("User not found.");

    private static void SetPassword(User user, string newPassword)
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.MustChangePassword = false;
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        ClearResetCode(user);
    }

    private static void ClearResetCode(User user)
    {
        user.PasswordResetCodeHash = null;
        user.PasswordResetExpiresAt = null;
        user.PasswordResetAttempts = 0;
    }

    private LoginResponseDto CreateLoginResponse(User user)
    {
        var (token, expiresAt) = _tokens.CreateToken(user);
        return new LoginResponseDto { Token = token, ExpiresAt = expiresAt, User = ToDto(user) };
    }

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        EmployeeCode = user.EmployeeCode,
        Email = user.Email,
        Role = user.Role.ToString(),
        DepartmentId = user.DepartmentId,
        DepartmentName = user.Department?.Name,
        MustChangePassword = user.MustChangePassword,
        Permissions = user.Permissions.Select(p => p.Permission.ToString()).ToList()
    };
}
