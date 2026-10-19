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

    public CamerasController(ICameraService cameras) => _cameras = cameras;

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
