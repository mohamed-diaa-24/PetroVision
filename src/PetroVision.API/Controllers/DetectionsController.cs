using Microsoft.AspNetCore.Mvc;
using PetroVision.API.Filters;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;

namespace PetroVision.API.Controllers;

// Called by the AI service only (X-Api-Key header), not by dashboard users.
[ApiController]
[ServiceFilter(typeof(ApiKeyAuthFilter))]
[Route("api/[controller]")]
public class DetectionsController : ControllerBase
{
    private readonly IDetectionService _detections;

    public DetectionsController(IDetectionService detections) => _detections = detections;

    [HttpPost]
    public async Task<ActionResult<DetectionResultDto>> Post(DetectionDto dto)
        => Ok(await _detections.ProcessAsync(dto));

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat(HeartbeatDto dto)
    {
        await _detections.HeartbeatAsync(dto);
        return NoContent();
    }
}
