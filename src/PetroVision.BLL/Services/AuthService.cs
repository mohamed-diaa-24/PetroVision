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
    private readonly PetroVisionDbContext _db;
    private readonly ITokenService _tokens;

    public AuthService(PetroVisionDbContext db, ITokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .Include(u => u.Permissions)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        switch (user.Status)
        {
            case UserStatus.Pending:
                throw new ForbiddenException("Your account is awaiting manager approval.");
            case UserStatus.Rejected:
                throw new ForbiddenException("Your sign-up request was rejected.");
            case UserStatus.Expired:
                throw new ForbiddenException("Your account has expired. Please contact your manager.");
        }

        user.LastActiveAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var (token, expiresAt) = _tokens.CreateToken(user);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = ToDto(user)
        };
    }

    public async Task<MessageResponseDto> SignupAsync(SignupRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var code = request.EmployeeCode.Trim();

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
            Role = UserRole.Agent,
            Status = UserStatus.Pending
        });

        await _db.SaveChangesAsync();

        return new MessageResponseDto { Message = "Sign-up request submitted. Waiting for manager approval." };
    }

    public async Task<UserDto> GetProfileAsync(int userId)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User not found.");

        return ToDto(user);
    }

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        EmployeeCode = user.EmployeeCode,
        Email = user.Email,
        Role = user.Role.ToString(),
        Permissions = user.Permissions.Select(p => p.Permission.ToString()).ToList()
    };
}
