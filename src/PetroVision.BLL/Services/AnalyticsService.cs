using Microsoft.EntityFrameworkCore;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Exceptions;
using PetroVision.BLL.Interfaces;
using PetroVision.DAL.Context;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Services;

public class AnalyticsService : IAnalyticsService
{
    private const int MaxDays = 90;
    private const int TopCamerasCount = 5;

    private readonly PetroVisionDbContext _db;
    private readonly ICurrentUser _user;

    public AnalyticsService(PetroVisionDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<AnalyticsDto> GetAsync(int days, int? departmentId)
    {
        if (days < 1 || days > MaxDays)
            throw new BadRequestException($"Days must be between 1 and {MaxDays}.");

        int? scope = _user.Role == "Owner"
            ? departmentId
            : _user.DepartmentId ?? throw new ForbiddenException("Your account is not assigned to a department yet.");

        var to = DateTime.UtcNow;
        var from = to.Date.AddDays(-(days - 1));          // whole days, today included
        var previousFrom = from.AddDays(-days);

        var alertsQuery = _db.Alerts.AsNoTracking().Where(a => a.CreatedAt >= previousFrom);
        var incidentsQuery = _db.Incidents.AsNoTracking().Where(i => i.CreatedAt >= from);
        if (scope.HasValue)
        {
            var departmentScope = scope.Value;
            alertsQuery = alertsQuery.Where(a => a.Camera.DepartmentId == departmentScope);
            incidentsQuery = incidentsQuery.Where(i => i.Camera.DepartmentId == departmentScope);
        }

        // Small projections; the grouping is done in memory (a few thousand rows at most per period).
        var alerts = await alertsQuery
            .Select(a => new { a.CreatedAt, a.HandledAt, a.Status, a.Type, a.CameraId, Camera = a.Camera.Name })
            .ToListAsync();
        var incidents = await incidentsQuery
            .Select(i => new { i.CreatedAt, i.Status, i.Type, i.CameraId, Camera = i.Camera.Name })
            .ToListAsync();

        // First handling of an incident = its "IncidentAcknowledged" / "IncidentClosed" audit entry.
        var handlings = _db.AuditLogs.AsNoTracking()
            .Where(l => l.CreatedAt >= from && l.EntityType == "Incident"
                        && (l.Action == AuditAction.IncidentAcknowledged || l.Action == AuditAction.IncidentClosed
                            || l.Action == AuditAction.IncidentReassigned))
            .Join(_db.Incidents, l => l.EntityId, i => (int?)i.Id,
                (l, i) => new { i.Id, i.CreatedAt, Handled = l.CreatedAt, i.Camera.DepartmentId });
        if (scope.HasValue)
        {
            var departmentScope = scope.Value;
            handlings = handlings.Where(x => x.DepartmentId == departmentScope);
        }
        var incidentAckTimes = await handlings.ToListAsync();

        var current = alerts.Where(a => a.CreatedAt >= from).ToList();
        var previousCount = alerts.Count(a => a.CreatedAt < from);

        var responseSeconds = current
            .Where(a => a.HandledAt.HasValue)
            .Select(a => (a.HandledAt!.Value - a.CreatedAt).TotalSeconds)
            .Concat(incidentAckTimes
                .GroupBy(x => x.Id)
                .Select(g => (g.Min(x => x.Handled) - g.First().CreatedAt).TotalSeconds))
            .Where(s => s >= 0)
            .ToList();

        var falseAlarms = current.Count(a => a.Status == AlertStatus.Dismissed);

        var categories = current.GroupBy(a => a.Type).Select(g => (Type: g.Key, Count: g.Count()))
            .Concat(incidents.Where(i => i.Type != IncidentType.Other)
                .GroupBy(i => (DetectionType)(int)i.Type).Select(g => (Type: g.Key, Count: g.Count())))
            .GroupBy(x => x.Type)
            .Select(g => (Type: g.Key, Count: g.Sum(x => x.Count)))
            .ToList();
        var categoryTotal = categories.Sum(c => c.Count);

        return new AnalyticsDto
        {
            From = from,
            To = to,
            Days = days,
            Warnings = current.Count,
            WarningsChangePercent = previousCount == 0
                ? null
                : Math.Round((current.Count - previousCount) * 100.0 / previousCount, 1),
            Incidents = incidents.Count,
            OpenIncidents = incidents.Count(i => i.Status != IncidentStatus.Closed),
            FalseAlarms = falseAlarms,
            FalseAlarmRate = current.Count == 0 ? 0 : Math.Round(falseAlarms * 100.0 / current.Count, 1),
            AvgAcknowledgeSeconds = responseSeconds.Count == 0 ? null : Math.Round(responseSeconds.Average()),

            EventsPerDay = Enumerable.Range(0, days)
                .Select(d => DateOnly.FromDateTime(from.AddDays(d)))
                .Select(day => new DailyEventsDto
                {
                    Date = day,
                    Alerts = current.Count(a => DateOnly.FromDateTime(a.CreatedAt) == day),
                    Incidents = incidents.Count(i => DateOnly.FromDateTime(i.CreatedAt) == day)
                })
                .ToList(),

            Categories = categories
                .OrderByDescending(c => c.Count)
                .Select(c => new CategoryShareDto
                {
                    Type = c.Type,
                    Count = c.Count,
                    Percent = categoryTotal == 0 ? 0 : Math.Round(c.Count * 100.0 / categoryTotal, 1)
                })
                .ToList(),

            TopCameras = current.Select(a => (a.CameraId, a.Camera))
                .Concat(incidents.Select(i => (i.CameraId, i.Camera)))
                .GroupBy(x => x.CameraId)
                .Select(g => new CameraEventsDto { CameraId = g.Key, Camera = g.First().Camera, Events = g.Count() })
                .OrderByDescending(c => c.Events)
                .Take(TopCamerasCount)
                .ToList()
        };
    }
}
