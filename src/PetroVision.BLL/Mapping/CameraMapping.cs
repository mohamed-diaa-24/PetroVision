using PetroVision.BLL.DTOs;
using PetroVision.DAL.Entities;

namespace PetroVision.BLL.Mapping;

public static class CameraMapping
{
    public static CameraDto ToDto(this Camera c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Location = c.Location,
        StreamUrl = c.StreamUrl,
        Health = c.Health,
        IsMuted = c.IsMuted,
        LastSeenAt = c.LastSeenAt,
        Models = c.ModelConfigs
            .OrderBy(m => m.Type)
            .Select(m => new CameraModelDto
            {
                Type = m.Type,
                IsEnabled = m.IsEnabled,
                ConfidenceThreshold = m.ConfidenceThreshold,
                CooldownMinutes = m.CooldownMinutes,
                EvidenceRetentionDays = m.EvidenceRetentionDays
            })
            .ToList()
    };
}
