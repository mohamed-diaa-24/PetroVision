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
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alerts;

    public AlertsController(IAlertService alerts) => _alerts = alerts;

    [HttpGet]
    public async Task<ActionResult<List<AlertDto>>> GetAll(
        [FromQuery] AlertStatus? status, [FromQuery] AlertLevel? level, [FromQuery] int? cameraId)
        => Ok(await _alerts.GetAllAsync(status, level, cameraId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AlertDto>> GetById(int id) => Ok(await _alerts.GetByIdAsync(id));

    [HttpPost("{id:int}/acknowledge")]
    public async Task<ActionResult<AlertDto>> Acknowledge(int id) => Ok(await _alerts.AcknowledgeAsync(id));

    [HttpPost("{id:int}/dismiss")]
    public async Task<ActionResult<AlertDto>> Dismiss(int id, DismissAlertDto dto)
        => Ok(await _alerts.DismissAsync(id, dto.Reason));

    [HttpPost("{id:int}/escalate")]
    public async Task<ActionResult<IncidentDto>> Escalate(int id) => Ok(await _alerts.EscalateAsync(id));
}
