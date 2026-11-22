using PetroVision.BLL.DTOs;

namespace PetroVision.BLL.Interfaces;

public interface IActivityService
{
    Task<List<AuditLogDto>> GetMineAsync(ActivityQuery query);
    Task<List<AuditLogDto>> GetTeamAsync(ActivityQuery query);
}
