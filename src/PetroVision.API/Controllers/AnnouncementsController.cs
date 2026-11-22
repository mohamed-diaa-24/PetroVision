using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetroVision.API.Extensions;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;

namespace PetroVision.API.Controllers;

[ApiController]
[Authorize(Policy = Policies.Staff)]
[Route("api/[controller]")]
public class AnnouncementsController : ControllerBase
{
    private readonly INotesService _notes;

    public AnnouncementsController(INotesService notes) => _notes = notes;

    // Pinned first, then newest. Includes "seen by X of Y".
    [HttpGet]
    public async Task<ActionResult<List<AnnouncementDto>>> GetAll() => Ok(await _notes.GetAnnouncementsAsync());

    [HttpPost]
    [Authorize(Policy = Policies.PostAnnouncements)]
    public async Task<ActionResult<AnnouncementDto>> Create(CreateAnnouncementDto dto)
        => Ok(await _notes.CreateAnnouncementAsync(dto));

    [HttpPost("{id:int}/seen")]
    public async Task<ActionResult<AnnouncementDto>> MarkSeen(int id) => Ok(await _notes.MarkAnnouncementSeenAsync(id));

    [HttpPatch("{id:int}/pin")]
    [Authorize(Policy = Policies.PostAnnouncements)]
    public async Task<ActionResult<AnnouncementDto>> Pin(int id, PinAnnouncementDto dto)
        => Ok(await _notes.SetPinnedAsync(id, dto.IsPinned));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.PostAnnouncements)]
    public async Task<IActionResult> Delete(int id)
    {
        await _notes.DeleteAnnouncementAsync(id);
        return NoContent();
    }
}
