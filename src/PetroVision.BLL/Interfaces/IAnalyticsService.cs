using PetroVision.BLL.DTOs;

namespace PetroVision.BLL.Interfaces;

public interface IAnalyticsService
{
    // Manager: own department. Owner: all departments, or one with departmentId.
    Task<AnalyticsDto> GetAsync(int days, int? departmentId);
}
