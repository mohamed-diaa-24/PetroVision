using PetroVision.BLL.DTOs;

namespace PetroVision.BLL.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetAsync();
}
