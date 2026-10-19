using PetroVision.BLL.DTOs;

namespace PetroVision.BLL.Interfaces;

public interface IDetectionService
{
    Task<DetectionResultDto> ProcessAsync(DetectionDto dto);
    Task HeartbeatAsync(HeartbeatDto dto);
}
