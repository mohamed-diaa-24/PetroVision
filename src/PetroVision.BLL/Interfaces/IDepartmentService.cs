using PetroVision.BLL.DTOs;

namespace PetroVision.BLL.Interfaces;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync();
}
