using Microsoft.EntityFrameworkCore;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;
using PetroVision.BLL.Mapping;
using PetroVision.DAL.Context;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Services;

public class DashboardService : IDashboardService
{
    private const int LatestEventsCount = 10;

    private readonly PetroVisionDbContext _db;

    public DashboardService(PetroVisionDbContext db) => _db = db;

    public async Task<DashboardDto> GetAsync()
    {
        var activeAlerts = await _db.Alerts.CountAsync(a => a.Status == AlertStatus.New);
        var openIncidents = await _db.Incidents.CountAsync(i => i.Status == IncidentStatus.Active);

        var cameras = await _db.Cameras
            .AsNoTracking()
            .Include(c => c.ModelConfigs)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var runningModels = cameras
            .Where(c => c.Health == CameraHealth.Online)
            .SelectMany(c => c.ModelConfigs)
            .Where(m => m.IsEnabled)
            .ToList();

        var alerts = await _db.Alerts
            .AsNoTracking()
            .Include(a => a.Camera)
            .OrderByDescending(a => a.CreatedAt)
            .Take(LatestEventsCount)
            .ToListAsync();

        var incidents = await _db.Incidents
            .AsNoTracking()
            .Include(i => i.Camera)
            .OrderByDescending(i => i.CreatedAt)
            .Take(LatestEventsCount)
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
            .Take(LatestEventsCount)
            .ToList();

        return new DashboardDto
        {
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
}
