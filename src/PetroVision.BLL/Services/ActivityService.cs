using Microsoft.EntityFrameworkCore;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Exceptions;
using PetroVision.BLL.Interfaces;
using PetroVision.DAL.Context;
using PetroVision.DAL.Entities;

namespace PetroVision.BLL.Services;

public class ActivityService : IActivityService
{
    private const int MaxTake = 500;

    private readonly PetroVisionDbContext _db;
    private readonly ICurrentUser _user;

    public ActivityService(PetroVisionDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public Task<List<AuditLogDto>> GetMineAsync(ActivityQuery query)
        => RunAsync(_db.AuditLogs.Where(l => l.UserId == _user.UserId), query);

    // Manager / agents with "View Team Logs": their department. Owner: every department.
    public Task<List<AuditLogDto>> GetTeamAsync(ActivityQuery query)
    {
        IQueryable<AuditLog> logs = _db.AuditLogs;

        if (_user.Role == "Owner")
        {
            if (query.DepartmentId.HasValue)
                logs = logs.Where(l => l.DepartmentId == query.DepartmentId.Value);
        }
        else
        {
            var departmentId = _user.DepartmentId
                ?? throw new ForbiddenException("Your account is not assigned to a department yet.");
            logs = logs.Where(l => l.DepartmentId == departmentId);
        }

        if (query.UserId.HasValue)
            logs = logs.Where(l => l.UserId == query.UserId.Value);

        return RunAsync(logs, query);
    }

    private static async Task<List<AuditLogDto>> RunAsync(IQueryable<AuditLog> logs, ActivityQuery query)
    {
        if (query.Action.HasValue) logs = logs.Where(l => l.Action == query.Action.Value);
        if (query.From.HasValue) logs = logs.Where(l => l.CreatedAt >= query.From.Value);
        if (query.To.HasValue) logs = logs.Where(l => l.CreatedAt <= query.To.Value);

        var take = Math.Clamp(query.Take, 1, MaxTake);

        return await logs
            .AsNoTracking()
            .OrderByDescending(l => l.CreatedAt)
            .Take(take)
            .Select(l => new AuditLogDto
            {
                Id = l.Id,
                Time = l.CreatedAt,
                UserId = l.UserId,
                User = l.User != null ? l.User.FullName : "System",
                Action = l.Action,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                Item = l.Item,
                Details = l.Details,
                Department = l.Department != null ? l.Department.Name : null
            })
            .ToListAsync();
    }
}
