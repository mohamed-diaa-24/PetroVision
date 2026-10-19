using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetroVision.API.Extensions;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;
using PetroVision.DAL.Enums;

namespace PetroVision.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly IIncidentService _incidents;

    public IncidentsController(IIncidentService incidents) => _incidents = incidents;

    // Read: Manager, Agent and Owner (Owner is read-only and sees all departments)
    [HttpGet]
    public async Task<ActionResult<List<IncidentDto>>> GetAll(
        [FromQuery] IncidentStatus? status, [FromQuery] IncidentType? type)
        => Ok(await _incidents.GetAllAsync(status, type));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IncidentDto>> GetById(int id) => Ok(await _incidents.GetByIdAsync(id));

    [HttpGet("assignees")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<List<AssigneeDto>>> GetAssignees() => Ok(await _incidents.GetAssigneesAsync());

    [HttpPost]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<IncidentDto>> Create(CreateIncidentDto dto)
    {
        var created = await _incidents.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:int}/acknowledge")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<IncidentDto>> Acknowledge(int id) => Ok(await _incidents.AcknowledgeAsync(id));

    [HttpPost("{id:int}/close")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<IncidentDto>> Close(int id, CloseIncidentDto dto)
        => Ok(await _incidents.CloseAsync(id, dto.Note));

    [HttpPost("{id:int}/reassign")]
    [Authorize(Policy = Policies.ReassignIncidents)]
    public async Task<ActionResult<IncidentDto>> Reassign(int id, ReassignIncidentDto dto)
        => Ok(await _incidents.ReassignAsync(id, dto.AssigneeId));
}
