using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetroVision.API.Extensions;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;

namespace PetroVision.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _activity;

    public ActivityController(IActivityService activity) => _activity = activity;

    // "My Activity": actions done by the signed-in user
    [HttpGet("mine")]
    public async Task<ActionResult<List<AuditLogDto>>> GetMine([FromQuery] ActivityQuery query)
        => Ok(await _activity.GetMineAsync(query));

    // "Team Logs": Manager, Owner, or agents with the View Team Logs permission
    [HttpGet("team")]
    [Authorize(Policy = Policies.ViewTeamLogs)]
    public async Task<ActionResult<List<AuditLogDto>>> GetTeam([FromQuery] ActivityQuery query)
        => Ok(await _activity.GetTeamAsync(query));
}
