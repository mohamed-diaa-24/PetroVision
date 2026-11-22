using PetroVision.BLL.DTOs;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Interfaces;

// Manager: the agents of their own department.
public interface IAgentService
{
    Task<List<AgentDto>> GetAllAsync(UserStatus? status);
    Task<AgentDto> GetByIdAsync(int id);
    Task<CreatedAgentDto> CreateAsync(CreateAgentDto dto);
    Task<AgentDto> SetPermissionsAsync(int id, List<PermissionType> permissions);
    Task<List<AgentDto>> ApplySeniorTemplateAsync();
    Task<AgentDto> EndServiceAsync(int id);
    Task<AgentDto> ReactivateAsync(int id);
}
