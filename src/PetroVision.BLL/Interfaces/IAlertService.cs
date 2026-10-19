using PetroVision.BLL.DTOs;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Interfaces;

public interface IAlertService
{
    Task<List<AlertDto>> GetAllAsync(AlertStatus? status, AlertLevel? level, int? cameraId);
    Task<AlertDto> GetByIdAsync(int id);
    Task<AlertDto> AcknowledgeAsync(int id);
    Task<AlertDto> DismissAsync(int id, DismissReason reason);
    Task<IncidentDto> EscalateAsync(int id);
}
