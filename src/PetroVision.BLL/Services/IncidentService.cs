using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Exceptions;
using PetroVision.BLL.Interfaces;
using PetroVision.BLL.Mapping;
using PetroVision.DAL.Context;
using PetroVision.DAL.Entities;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Services;

public class IncidentService : IIncidentService
{
    private readonly PetroVisionDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IAuditLogger _audit;
    private readonly IEventNotifier _events;
    private readonly ILogger<IncidentService> _logger;

    public IncidentService(PetroVisionDbContext db, ICurrentUser user, IAuditLogger audit,
        IEventNotifier events, ILogger<IncidentService> logger)
    {
        _db = db;
        _user = user;
        _audit = audit;
        _events = events;
        _logger = logger;
    }

    private int DepartmentId => _user.DepartmentId
        ?? throw new ForbiddenException("Your account is not assigned to a department yet.");

    // Owner can read incidents of all departments (read-only). Everyone else: own department.
    private IQueryable<Incident> Scoped(bool forWrite = false)
    {
        if (_user.Role == "Owner" && !forWrite)
            return _db.Incidents;

        var departmentId = DepartmentId;
        return _db.Incidents.Where(i => i.Camera.DepartmentId == departmentId);
    }

    private async Task<Incident> LoadForWriteAsync(int id)
        => await Scoped(forWrite: true).FirstOrDefaultAsync(i => i.Id == id)
           ?? throw new NotFoundException("Incident not found.");

    public async Task<List<IncidentDto>> GetAllAsync(IncidentStatus? status, IncidentType? type)
    {
        IQueryable<Incident> query = Scoped().AsNoTracking().WithDetails();

        if (status.HasValue) query = query.Where(i => i.Status == status.Value);
        if (type.HasValue) query = query.Where(i => i.Type == type.Value);

        var incidents = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
        return incidents.Select(i => i.ToDto()).ToList();
    }

    public async Task<IncidentDto> GetByIdAsync(int id)
    {
        var incident = await Scoped().AsNoTracking().WithDetails().FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new NotFoundException("Incident not found.");

        return incident.ToDto();
    }

    public async Task<List<AssigneeDto>> GetAssigneesAsync()
    {
        var departmentId = DepartmentId;

        return await _db.Users
            .AsNoTracking()
            .Where(u => u.DepartmentId == departmentId
                        && u.Status == UserStatus.Active
                        && (u.Role == UserRole.Manager || u.Role == UserRole.Agent))
            .OrderBy(u => u.FullName)
            .Select(u => new AssigneeDto { Id = u.Id, FullName = u.FullName, Role = u.Role.ToString() })
            .ToListAsync();
    }

    public async Task<IncidentDto> CreateAsync(CreateIncidentDto dto)
    {
        var departmentId = DepartmentId;

        var camera = await _db.Cameras
            .FirstOrDefaultAsync(c => c.Id == dto.CameraId && c.DepartmentId == departmentId && !c.IsDeleted)
            ?? throw new NotFoundException("Camera not found.");

        var incident = new Incident
        {
            CameraId = camera.Id,
            Type = dto.Type,
            Status = IncidentStatus.Active,
            Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim()
        };

        _db.Incidents.Add(incident);
        await _db.SaveChangesAsync();

        _audit.Add(AuditAction.IncidentCreated, nameof(Incident), incident.Id, $"Incident #{incident.Id}",
            $"{incident.Type} · {camera.Name}");
        await _db.SaveChangesAsync();

        var result = await GetByIdAsync(incident.Id);
        await _events.TryNotifyAsync(_logger, "IncidentCreated", n => n.IncidentCreatedAsync(departmentId, result));
        return result;
    }

    public async Task<IncidentDto> AcknowledgeAsync(int id)
    {
        var incident = await LoadForWriteAsync(id);

        if (incident.Status != IncidentStatus.Active)
            throw new ConflictException("Only active incidents can be acknowledged.");

        incident.Status = IncidentStatus.Acknowledged;
        incident.HandlerId = _user.UserId;

        _audit.Add(AuditAction.IncidentAcknowledged, nameof(Incident), incident.Id, $"Incident #{incident.Id}");
        await _db.SaveChangesAsync();
        return await UpdatedAsync(id);
    }

    public async Task<IncidentDto> CloseAsync(int id, string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            throw new BadRequestException("A resolution note is required before closing this incident.");

        var incident = await LoadForWriteAsync(id);

        if (incident.Status == IncidentStatus.Closed)
            throw new ConflictException("This incident is already closed.");

        incident.Status = IncidentStatus.Closed;
        incident.Note = note.Trim();
        incident.ClosedAt = DateTime.UtcNow;
        incident.HandlerId ??= _user.UserId;

        _audit.Add(AuditAction.IncidentClosed, nameof(Incident), incident.Id, $"Incident #{incident.Id}", incident.Note);
        await _db.SaveChangesAsync();
        return await UpdatedAsync(id);
    }

    public async Task<IncidentDto> ReassignAsync(int id, int assigneeId)
    {
        var departmentId = DepartmentId;
        var incident = await LoadForWriteAsync(id);

        if (incident.Status == IncidentStatus.Closed)
            throw new ConflictException("Closed incidents cannot be reassigned.");

        var assignee = await _db.Users.FirstOrDefaultAsync(u => u.Id == assigneeId
            && u.DepartmentId == departmentId
            && u.Status == UserStatus.Active
            && (u.Role == UserRole.Manager || u.Role == UserRole.Agent))
            ?? throw new BadRequestException("Assignee must be an active member of your department.");

        incident.HandlerId = assignee.Id;
        incident.Status = IncidentStatus.Acknowledged;

        _audit.Add(AuditAction.IncidentReassigned, nameof(Incident), incident.Id, $"Incident #{incident.Id}",
            $"Assigned to {assignee.FullName}");
        await _db.SaveChangesAsync();
        return await UpdatedAsync(id);
    }

    // Reloads the incident after a saved change and pushes IncidentUpdated (department + owners).
    private async Task<IncidentDto> UpdatedAsync(int id)
    {
        var result = await GetByIdAsync(id);
        await _events.TryNotifyAsync(_logger, "IncidentUpdated", n => n.IncidentUpdatedAsync(DepartmentId, result));
        return result;
    }
}
