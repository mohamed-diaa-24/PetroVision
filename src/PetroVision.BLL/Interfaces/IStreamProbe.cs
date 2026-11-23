using PetroVision.BLL.DTOs;

namespace PetroVision.BLL.Interfaces;

// Checks that a camera stream URL is reachable before the camera is saved.
public interface IStreamProbe
{
    Task<ConnectionTestResultDto> TestAsync(string streamUrl, CancellationToken ct = default);
}
