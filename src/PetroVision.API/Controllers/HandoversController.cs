using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetroVision.API.Extensions;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;

namespace PetroVision.API.Controllers;

// Shift handover notes of the user's department (Manager and Agents).
[ApiController]
[Authorize(Policy = Policies.Staff)]
[Route("api/[controller]")]
public class HandoversController : ControllerBase
{
    private readonly INotesService _notes;

    public HandoversController(INotesService notes) => _notes = notes;

    [HttpGet]
    public async Task<ActionResult<List<HandoverNoteDto>>> GetAll([FromQuery] int take = 50)
        => Ok(await _notes.GetHandoversAsync(take));

    [HttpPost]
    public async Task<ActionResult<HandoverNoteDto>> Create(CreateHandoverNoteDto dto)
        => Ok(await _notes.CreateHandoverAsync(dto));

    [HttpPost("{id:int}/read")]
    public async Task<ActionResult<HandoverNoteDto>> MarkRead(int id) => Ok(await _notes.MarkHandoverReadAsync(id));
}
