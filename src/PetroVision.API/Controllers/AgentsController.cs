using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetroVision.API.Extensions;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;
using PetroVision.DAL.Enums;

namespace PetroVision.API.Controllers;

// Management Team > Agents / Permissions (Manager, own department)
[ApiController]
[Authorize(Policy = Policies.ManagerOnly)]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly IAgentService _agents;

    public AgentsController(IAgentService agents) => _agents = agents;

    [HttpGet]
    public async Task<ActionResult<List<AgentDto>>> GetAll([FromQuery] UserStatus? status)
        => Ok(await _agents.GetAllAsync(status));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AgentDto>> GetById(int id) => Ok(await _agents.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<CreatedAgentDto>> Create(CreateAgentDto dto)
    {
        var created = await _agents.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Agent.Id }, created);
    }

    [HttpPut("{id:int}/permissions")]
    public async Task<ActionResult<AgentDto>> SetPermissions(int id, UpdatePermissionsDto dto)
        => Ok(await _agents.SetPermissionsAsync(id, dto.Permissions));

    [HttpPost("permissions/senior-template")]
    public async Task<ActionResult<List<AgentDto>>> ApplySeniorTemplate() => Ok(await _agents.ApplySeniorTemplateAsync());

    [HttpPost("{id:int}/end-service")]
    public async Task<ActionResult<AgentDto>> EndService(int id) => Ok(await _agents.EndServiceAsync(id));

    [HttpPost("{id:int}/reactivate")]
    public async Task<ActionResult<AgentDto>> Reactivate(int id) => Ok(await _agents.ReactivateAsync(id));
}
