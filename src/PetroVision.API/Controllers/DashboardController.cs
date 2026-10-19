using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetroVision.API.Extensions;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;

namespace PetroVision.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    // Manager + Agent: their own department
    [HttpGet]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<DashboardDto>> Get() => Ok(await _dashboard.GetOperationsAsync());

    // Owner: all departments
    [HttpGet("owner")]
    [Authorize(Policy = Policies.OwnerOnly)]
    public async Task<ActionResult<OwnerDashboardDto>> GetOwner() => Ok(await _dashboard.GetOwnerAsync());
}
