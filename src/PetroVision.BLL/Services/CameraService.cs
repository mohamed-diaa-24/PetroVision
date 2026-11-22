using Microsoft.EntityFrameworkCore;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Exceptions;
using PetroVision.BLL.Interfaces;
using PetroVision.BLL.Mapping;
using PetroVision.DAL.Context;
using PetroVision.DAL.Entities;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Services;

public class CameraService : ICameraService
{
    private readonly PetroVisionDbContext _db;
    private readonly ICurrentUser _user;

    public CameraService(PetroVisionDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    // Every query is limited to the current user's department and hides removed cameras.
    private IQueryable<Camera> Scoped()
    {
        var departmentId = _user.DepartmentId
            ?? throw new ForbiddenException("Your account is not assigned to a department yet.");

        return _db.Cameras.Where(c => c.DepartmentId == departmentId && !c.IsDeleted);
    }

    public async Task<List<CameraDto>> GetAllAsync()
    {
        var cameras = await Scoped()
            .AsNoTracking()
            .Include(c => c.ModelConfigs)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return cameras.Select(c => c.ToDto()).ToList();
    }

    public async Task<CameraDto> GetByIdAsync(int id)
    {
        var camera = await Scoped()
            .AsNoTracking()
            .Include(c => c.ModelConfigs)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException("Camera not found.");

        return camera.ToDto();
    }

    public async Task<CameraDto> CreateAsync(CreateCameraDto dto)
    {
        var departmentId = _user.DepartmentId
            ?? throw new ForbiddenException("Your account is not assigned to a department yet.");

        var name = dto.Name.Trim();

        if (await _db.Cameras.AnyAsync(c => c.Name == name))
            throw new ConflictException("A camera with this name already exists.");

        var camera = new Camera
        {
            Name = name,
            Location = dto.Location.Trim(),
            StreamUrl = dto.StreamUrl.Trim(),
            DepartmentId = departmentId,
            Health = CameraHealth.Offline,
            ModelConfigs = dto.Models
                .GroupBy(m => m.Type)
                .Select(g => g.Last())
                .Select(ToConfig)
                .ToList()
        };

        _db.Cameras.Add(camera);
        await _db.SaveChangesAsync();

        return camera.ToDto();
    }

    public async Task<CameraDto> UpdateModelsAsync(int id, List<CameraModelDto> models)
    {
        var camera = await Scoped()
            .Include(c => c.ModelConfigs)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException("Camera not found.");

        var incoming = models.GroupBy(m => m.Type).Select(g => g.Last()).ToList();

        var toRemove = camera.ModelConfigs
            .Where(existing => incoming.All(m => m.Type != existing.Type))
            .ToList();
        _db.CameraModelConfigs.RemoveRange(toRemove);

        foreach (var m in incoming)
        {
            var existing = camera.ModelConfigs.FirstOrDefault(e => e.Type == m.Type);

            if (existing is null)
            {
                camera.ModelConfigs.Add(ToConfig(m));
            }
            else
            {
                existing.IsEnabled = m.IsEnabled;
                existing.ConfidenceThreshold = m.ConfidenceThreshold;
                existing.CooldownMinutes = m.CooldownMinutes;
                existing.EvidenceRetentionDays = m.EvidenceRetentionDays;
            }
        }

        await _db.SaveChangesAsync();
        return camera.ToDto();
    }

    public async Task<CameraDto> SetMuteAsync(int id, bool isMuted)
    {
        var camera = await Scoped()
            .Include(c => c.ModelConfigs)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException("Camera not found.");

        camera.IsMuted = isMuted;
        await _db.SaveChangesAsync();

        return camera.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var camera = await Scoped().FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException("Camera not found.");

        // Soft delete: alerts and incidents keep pointing at this camera for the audit trail.
        camera.IsDeleted = true;
        camera.DeletedAt = DateTime.UtcNow;
        camera.Health = CameraHealth.Offline;
        await _db.SaveChangesAsync();
    }

    private static CameraModelConfig ToConfig(CameraModelDto m) => new()
    {
        Type = m.Type,
        IsEnabled = m.IsEnabled,
        ConfidenceThreshold = m.ConfidenceThreshold,
        CooldownMinutes = m.CooldownMinutes,
        EvidenceRetentionDays = m.EvidenceRetentionDays
    };
}
