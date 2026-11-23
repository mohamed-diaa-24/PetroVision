using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetroVision.API.Extensions;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;
using PetroVision.DAL.Enums;

namespace PetroVision.API.Controllers;

[ApiController]
[Authorize(Policy = Policies.Staff)]
[Route("api/[controller]")]
public class CameraRequestsController : ControllerBase
{
    private readonly ICameraRequestService _requests;

    public CameraRequestsController(ICameraRequestService requests) => _requests = requests;

    // Agent (or Manager) submits an add / edit / remove request
    [HttpPost]
    public async Task<ActionResult<CameraRequestDto>> Create(CreateCameraRequestDto dto)
    {
        var created = await _requests.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<CameraRequestDto>>> GetMine() => Ok(await _requests.GetMineAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CameraRequestDto>> GetById(int id) => Ok(await _requests.GetByIdAsync(id));

    // Manager: department requests (Management Team > Camera Requests)
    [HttpGet]
    [Authorize(Policy = Policies.ManagerOnly)]
    public async Task<ActionResult<List<CameraRequestDto>>> GetDepartment([FromQuery] RequestStatus? status)
        => Ok(await _requests.GetDepartmentAsync(status));

    [HttpPost("{id:int}/approve")]
    [Authorize(Policy = Policies.ManagerOnly)]
    public async Task<ActionResult<CameraRequestDto>> Approve(int id) => Ok(await _requests.ApproveAsync(id));

    [HttpPost("{id:int}/reject")]
    [Authorize(Policy = Policies.ManagerOnly)]
    public async Task<ActionResult<CameraRequestDto>> Reject(int id, RejectRequestDto dto)
        => Ok(await _requests.RejectAsync(id, dto.Reason));
}
