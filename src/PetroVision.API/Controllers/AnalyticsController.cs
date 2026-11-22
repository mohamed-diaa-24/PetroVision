using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetroVision.API.Extensions;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;

namespace PetroVision.API.Controllers;

[ApiController]
[Authorize(Policy = Policies.Reviewers)]   // Manager (own department) or Owner
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analytics;

    public AnalyticsController(IAnalyticsService analytics) => _analytics = analytics;

    // ?days=7 (1..90). departmentId is used only for the Owner.
    [HttpGet]
    public async Task<ActionResult<AnalyticsDto>> Get([FromQuery] int days = 7, [FromQuery] int? departmentId = null)
        => Ok(await _analytics.GetAsync(days, departmentId));
}
