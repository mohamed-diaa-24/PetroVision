using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetroVision.API.Extensions;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;

namespace PetroVision.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CamerasController : ControllerBase
{
    private readonly ICameraService _cameras;
    private readonly IStreamProbe _probe;

    public CamerasController(ICameraService cameras, IStreamProbe probe)
    {
        _cameras = cameras;
        _probe = probe;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<List<CameraDto>>> GetAll() => Ok(await _cameras.GetAllAsync());

    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<CameraDto>> GetById(int id) => Ok(await _cameras.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Policy = Policies.ManageCameras)]
    public async Task<ActionResult<CameraDto>> Create(CreateCameraDto dto)
    {
        var created = await _cameras.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.ManageCameras)]
    public async Task<ActionResult<CameraDto>> Update(int id, UpdateCameraDto dto)
        => Ok(await _cameras.UpdateAsync(id, dto));

    // Staff (not only ManageCameras): agents test the URL before sending a camera request.
    [HttpPost("test-connection")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<ConnectionTestResultDto>> TestConnection(TestConnectionDto dto, CancellationToken ct)
        => Ok(await _probe.TestAsync(dto.StreamUrl, ct));

    [HttpPut("{id:int}/models")]
    [Authorize(Policy = Policies.EditModelSettings)]
    public async Task<ActionResult<CameraDto>> UpdateModels(int id, List<CameraModelDto> models)
        => Ok(await _cameras.UpdateModelsAsync(id, models));

    [HttpPatch("{id:int}/mute")]
    [Authorize(Policy = Policies.ManageCameras)]
    public async Task<ActionResult<CameraDto>> SetMute(int id, MuteCameraDto dto)
        => Ok(await _cameras.SetMuteAsync(id, dto.IsMuted));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.ManageCameras)]
    public async Task<IActionResult> Delete(int id)
    {
        await _cameras.DeleteAsync(id);
        return NoContent();
    }
}
