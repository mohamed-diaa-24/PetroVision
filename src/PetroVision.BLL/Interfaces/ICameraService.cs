using PetroVision.BLL.DTOs;

namespace PetroVision.BLL.Interfaces;

public interface ICameraService
{
    Task<List<CameraDto>> GetAllAsync();
    Task<CameraDto> GetByIdAsync(int id);
    Task<CameraDto> CreateAsync(CreateCameraDto dto);
    Task<CameraDto> UpdateAsync(int id, UpdateCameraDto dto);
    Task<CameraDto> UpdateModelsAsync(int id, List<CameraModelDto> models);
    Task<CameraDto> SetMuteAsync(int id, bool isMuted);
    Task DeleteAsync(int id);
}
