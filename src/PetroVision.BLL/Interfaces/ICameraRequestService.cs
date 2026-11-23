using PetroVision.BLL.DTOs;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Interfaces;

public interface ICameraRequestService
{
    Task<CameraRequestDto> CreateAsync(CreateCameraRequestDto dto);
    Task<List<CameraRequestDto>> GetMineAsync();
    Task<List<CameraRequestDto>> GetDepartmentAsync(RequestStatus? status);
    Task<CameraRequestDto> GetByIdAsync(int id);
    Task<CameraRequestDto> ApproveAsync(int id);
    Task<CameraRequestDto> RejectAsync(int id, string reason);
}
