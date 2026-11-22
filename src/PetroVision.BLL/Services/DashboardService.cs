using Microsoft.EntityFrameworkCore;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Exceptions;
using PetroVision.BLL.Interfaces;
using PetroVision.BLL.Mapping;
using PetroVision.DAL.Context;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Services;

public class DashboardService : IDashboardService
{
    private const int LatestCount = 10;

    private readonly PetroVisionDbContext _db;
    private readonly ICurrentUser _user;

    public DashboardService(PetroVisionDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    // ---------- Manager / Agent ----------
    public async Task<DashboardDto> GetOperationsAsync()
    {
        var departmentId = _user.DepartmentId
            ?? throw new ForbiddenException("Your account is not assigned to a department yet.");

        var department = await _db.Departments.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == departmentId)
            ?? throw new NotFoundException("Department not found.");

        var cameras = await _db.Cameras
            .AsNoTracking()
            .Include(c => c.ModelConfigs)
            .Where(c => c.DepartmentId == departmentId && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var activeAlerts = await _db.Alerts
            .CountAsync(a => a.Status == AlertStatus.New && a.Camera.DepartmentId == departmentId);

        var openIncidents = await _db.Incidents
            .CountAsync(i => i.Status != IncidentStatus.Closed && i.Camera.DepartmentId == departmentId);

        var runningModels = cameras
            .Where(c => c.Health == CameraHealth.Online)
            .SelectMany(c => c.ModelConfigs)
            .Where(m => m.IsEnabled)
            .ToList();

        var alerts = await _db.Alerts
            .AsNoTracking()
            .Include(a => a.Camera)
            .Where(a => a.Camera.DepartmentId == departmentId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(LatestCount)
            .ToListAsync();

        var incidents = await _db.Incidents
            .AsNoTracking()
            .Include(i => i.Camera)
            .Where(i => i.Camera.DepartmentId == departmentId)
            .OrderByDescending(i => i.CreatedAt)
            .Take(LatestCount)
            .ToListAsync();

        var events = alerts
            .Select(a => new EventDto
            {
                Id = a.Id,
                Kind = "Alert",
                Title = a.Type == DetectionType.PPE ? "PPE missing" : $"{a.Type} alert",
                Description = $"{a.Camera.Name} · {a.Camera.Location}",
                Level = a.Level.ToString().ToUpperInvariant(),
                Time = a.CreatedAt
            })
            .Concat(incidents.Select(i => new EventDto
            {
                Id = i.Id,
                Kind = "Incident",
                Title = $"{i.Type} detected",
                Description = $"{i.Camera.Name} · {i.Camera.Location}",
                Level = "INCIDENT",
                Time = i.CreatedAt
            }))
            .OrderByDescending(e => e.Time)
            .Take(LatestCount)
            .ToList();

        return new DashboardDto
        {
            DepartmentName = department.Name,
            NeedsSetup = cameras.Count == 0,
            ActiveAlerts = activeAlerts,
            OpenIncidents = openIncidents,
            CamerasOnline = cameras.Count(c => c.Health == CameraHealth.Online),
            CamerasTotal = cameras.Count,
            ModelsRunning = runningModels.Count,
            ModelTypes = runningModels.Select(m => m.Type).Distinct().OrderBy(t => t).ToList(),
            LatestEvents = events,
            Cameras = cameras.Select(c => c.ToDto()).ToList()
        };
    }

    // ---------- Owner ----------
    public async Task<OwnerDashboardDto> GetOwnerAsync()
    {
        var pendingManagers = await _db.Users
            .AsNoTracking()
            .Include(u => u.Department)
            .Where(u => u.Role == UserRole.Manager && u.Status == UserStatus.Pending)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

        var recentIncidents = await _db.Incidents
            .AsNoTracking()
            .Include(i => i.Camera).ThenInclude(c => c.Department)
            .OrderByDescending(i => i.CreatedAt)
            .Take(LatestCount)
            .ToListAsync();

        return new OwnerDashboardDto
        {
            Departments = await _db.Departments.CountAsync(),
            ManagersPending = pendingManagers.Count,
            PendingRequests = pendingManagers.Count,
            MajorIncidents = await _db.Incidents.CountAsync(),
            IncidentsRequiringAttention = await _db.Incidents.CountAsync(i => i.Status == IncidentStatus.Active),
            DevicesOnline = await _db.Cameras.CountAsync(c => !c.IsDeleted && c.Health == CameraHealth.Online),
            DevicesTotal = await _db.Cameras.CountAsync(c => !c.IsDeleted),
            ManagerRequests = pendingManagers.Select(u => new ManagerRequestDto
            {
                UserId = u.Id,
                Applicant = u.FullName,
                Department = u.Department?.Name ?? "—",
                Status = u.Status
            }).ToList(),
            RecentIncidents = recentIncidents.Select(i => new OwnerIncidentDto
            {
                Id = i.Id,
                Type = i.Type,
                Camera = i.Camera.Name,
                Department = i.Camera.Department.Name,
                Status = i.Status,
                Time = i.CreatedAt
            }).ToList()
        };
    }
}
