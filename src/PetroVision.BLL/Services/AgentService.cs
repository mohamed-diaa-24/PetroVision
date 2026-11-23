using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Exceptions;
using PetroVision.BLL.Interfaces;
using PetroVision.DAL.Context;
using PetroVision.DAL.Entities;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Services;

public class AgentService : IAgentService
{
    // Last request within this window = "online".
    private static readonly TimeSpan OnlineWindow = TimeSpan.FromMinutes(5);

    private readonly PetroVisionDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IAuditLogger _audit;

    public AgentService(PetroVisionDbContext db, ICurrentUser user, IAuditLogger audit)
    {
        _db = db;
        _user = user;
        _audit = audit;
    }

    private int DepartmentId => _user.DepartmentId
        ?? throw new ForbiddenException("Your account is not assigned to a department yet.");

    private IQueryable<User> Agents()
    {
        var departmentId = DepartmentId;
        return _db.Users.Include(u => u.Permissions)
            .Where(u => u.Role == UserRole.Agent && u.DepartmentId == departmentId);
    }

    private async Task<User> LoadAsync(int id)
        => await Agents().FirstOrDefaultAsync(u => u.Id == id)
           ?? throw new NotFoundException("Agent not found.");

    public async Task<List<AgentDto>> GetAllAsync(UserStatus? status)
    {
        var query = Agents().AsNoTracking();

        // Pending / rejected people are sign-up requests, not team members.
        query = status.HasValue
            ? query.Where(u => u.Status == status.Value)
            : query.Where(u => u.Status == UserStatus.Active || u.Status == UserStatus.Expired);

        var agents = await query.OrderBy(u => u.FullName).ToListAsync();
        return agents.Select(ToDto).ToList();
    }

    public async Task<AgentDto> GetByIdAsync(int id)
    {
        var agent = await Agents().AsNoTracking().FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new NotFoundException("Agent not found.");
        return ToDto(agent);
    }

    public async Task<CreatedAgentDto> CreateAsync(CreateAgentDto dto)
    {
        var departmentId = DepartmentId;
        var email = dto.Email.Trim().ToLowerInvariant();
        var code = dto.EmployeeCode.Trim();

        if (await _db.Users.AnyAsync(u => u.EmployeeCode == code))
            throw new ConflictException("Employee ID already used.");

        if (await _db.Users.AnyAsync(u => u.Email == email))
            throw new ConflictException("This email is already registered.");

        var temporaryPassword = GenerateTemporaryPassword();
        var agent = new User
        {
            FullName = dto.FullName.Trim(),
            EmployeeCode = code,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword),
            Role = UserRole.Agent,
            Status = UserStatus.Active,
            DepartmentId = departmentId,
            MustChangePassword = true,
            Permissions = dto.Permissions.Distinct().Select(p => new AgentPermission { Permission = p }).ToList()
        };

        _db.Users.Add(agent);
        await _db.SaveChangesAsync();

        _audit.Add(AuditAction.AgentCreated, nameof(User), agent.Id, $"{agent.FullName} ({agent.EmployeeCode})",
            agent.Permissions.Count > 0 ? "Permissions: " + string.Join(", ", agent.Permissions.Select(p => p.Permission)) : null);
        await _db.SaveChangesAsync();

        return new CreatedAgentDto { Agent = ToDto(agent), TemporaryPassword = temporaryPassword };
    }

    public async Task<AgentDto> SetPermissionsAsync(int id, List<PermissionType> permissions)
    {
        var agent = await LoadAsync(id);
        ReplacePermissions(agent, permissions);

        _audit.Add(AuditAction.PermissionsUpdated, nameof(User), agent.Id, agent.FullName, DescribePermissions(permissions));
        await _db.SaveChangesAsync();

        return ToDto(agent);
    }

    // "Senior Agent" = every permission, for every active agent of the department.
    public async Task<List<AgentDto>> ApplySeniorTemplateAsync()
    {
        var all = Enum.GetValues<PermissionType>().ToList();
        var agents = await Agents().Where(u => u.Status == UserStatus.Active).ToListAsync();

        foreach (var agent in agents)
        {
            ReplacePermissions(agent, all);
            _audit.Add(AuditAction.PermissionsUpdated, nameof(User), agent.Id, agent.FullName, "Senior Agent template");
        }

        await _db.SaveChangesAsync();
        return agents.OrderBy(a => a.FullName).Select(ToDto).ToList();
    }

    public async Task<AgentDto> EndServiceAsync(int id)
    {
        var agent = await LoadAsync(id);
        if (agent.Status != UserStatus.Active)
            throw new ConflictException("Only active agents can be ended.");

        agent.Status = UserStatus.Expired;
        agent.SecurityStamp = NewStamp();   // signs the agent out everywhere

        _audit.Add(AuditAction.ServiceEnded, nameof(User), agent.Id, agent.FullName);
        await _db.SaveChangesAsync();

        return ToDto(agent);
    }

    public async Task<AgentDto> ReactivateAsync(int id)
    {
        var agent = await LoadAsync(id);
        if (agent.Status != UserStatus.Expired)
            throw new ConflictException("Only expired agents can be reactivated.");

        agent.Status = UserStatus.Active;
        agent.SecurityStamp = NewStamp();

        _audit.Add(AuditAction.AccountReactivated, nameof(User), agent.Id, agent.FullName);
        await _db.SaveChangesAsync();

        return ToDto(agent);
    }

    // ---------- helpers ----------
    private void ReplacePermissions(User agent, IEnumerable<PermissionType> permissions)
    {
        var wanted = permissions.Distinct().ToList();

        _db.AgentPermissions.RemoveRange(agent.Permissions.Where(p => !wanted.Contains(p.Permission)).ToList());
        foreach (var p in wanted.Where(p => agent.Permissions.All(x => x.Permission != p)))
            agent.Permissions.Add(new AgentPermission { Permission = p });

        agent.SecurityStamp = NewStamp();   // old token loses the old permission claims
    }

    private static string DescribePermissions(List<PermissionType> permissions)
        => permissions.Count == 0 ? "No permissions" : "Permissions: " + string.Join(", ", permissions.Distinct());

    private static string NewStamp() => Guid.NewGuid().ToString("N");

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var body = RandomNumberGenerator.GetString(chars, 8);
        return $"PV-{body}!";
    }

    private static AgentDto ToDto(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        EmployeeCode = u.EmployeeCode,
        Email = u.Email,
        Status = u.Status,
        IsOnline = u.Status == UserStatus.Active && u.LastActiveAt.HasValue
                   && DateTime.UtcNow - u.LastActiveAt.Value <= OnlineWindow,
        LastActiveAt = u.LastActiveAt,
        CreatedAt = u.CreatedAt,
        Permissions = u.Permissions.Select(p => p.Permission).OrderBy(p => p).ToList()
    };
}
