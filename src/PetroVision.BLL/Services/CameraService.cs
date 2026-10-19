using Microsoft.EntityFrameworkCore;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Exceptions;
using PetroVision.BLL.Interfaces;
using PetroVision.BLL.Mapping;
using PetroVision.DAL.Context;
using PetroVision.DAL.Entities;

namespace PetroVision.BLL.Services;

public class CameraService : ICameraService
{
    private readonly PetroVisionDbContext _db;

    public CameraService(PetroVisionDbContext db) => _db = db;

    public async Task<List<CameraDto>> GetAllAsync()
    {
        var cameras = await _db.Cameras
            .AsNoTracking()
            .Include(c => c.ModelConfigs)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return cameras.Select(c => c.ToDto()).ToList();
    }

    public async Task<CameraDto> GetByIdAsync(int id)
    {
        var camera = await _db.Cameras
            .AsNoTracking()
            .Include(c => c.ModelConfigs)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException("Camera not found.");

        return camera.ToDto();
    }

    public async Task<CameraDto> CreateAsync(CreateCameraDto dto)
    {
        var name = dto.Name.Trim();

        if (await _db.Cameras.AnyAsync(c => c.Name == name))
            throw new ConflictException("A camera with this name already exists.");

        var camera = new Camera
        {
            Name = name,
            Location = dto.Location.Trim(),
            StreamUrl = dto.StreamUrl.Trim(),
            Health = Camera_DefaultHealth,
            ModelConfigs = dto.Models
                .GroupBy(m => m.Type)
                .Select(g => g.Last())
                .Select(m => new CameraModelConfig
                {
                    Type = m.Type,
                    IsEnabled = m.IsEnabled,
                    ConfidenceThreshold = m.ConfidenceThreshold,
                    CooldownMinutes = m.CooldownMinutes,
                    EvidenceRetentionDays = m.EvidenceRetentionDays
                })
                .ToList()
        };

        _db.Cameras.Add(camera);
        await _db.SaveChangesAsync();

        return camera.ToDto();
    }

    public async Task<CameraDto> UpdateModelsAsync(int id, List<CameraModelDto> models)
    {
        var camera = await _db.Cameras
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
                camera.ModelConfigs.Add(new CameraModelConfig
                {
                    Type = m.Type,
                    IsEnabled = m.IsEnabled,
                    ConfidenceThreshold = m.ConfidenceThreshold,
                    CooldownMinutes = m.CooldownMinutes,
                    EvidenceRetentionDays = m.EvidenceRetentionDays
                });
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
        var camera = await _db.Cameras
            .Include(c => c.ModelConfigs)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException("Camera not found.");

        camera.IsMuted = isMuted;
        await _db.SaveChangesAsync();

        return camera.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var camera = await _db.Cameras.FindAsync(id)
            ?? throw new NotFoundException("Camera not found.");

        _db.Cameras.Remove(camera);
        await _db.SaveChangesAsync();
    }

    private const PetroVision.DAL.Enums.CameraHealth Camera_DefaultHealth = PetroVision.DAL.Enums.CameraHealth.Offline;
}
