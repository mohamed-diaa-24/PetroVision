using PetroVision.BLL.DTOs;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Interfaces;

public interface IIncidentService
{
    Task<List<IncidentDto>> GetAllAsync(IncidentStatus? status, IncidentType? type);
    Task<IncidentDto> GetByIdAsync(int id);
    Task<List<AssigneeDto>> GetAssigneesAsync();
    Task<IncidentDto> CreateAsync(CreateIncidentDto dto);
    Task<IncidentDto> AcknowledgeAsync(int id);
    Task<IncidentDto> CloseAsync(int id, string note);
    Task<IncidentDto> ReassignAsync(int id, int assigneeId);
}
