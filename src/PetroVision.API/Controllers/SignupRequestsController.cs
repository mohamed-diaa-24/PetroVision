using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetroVision.API.Extensions;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;

namespace PetroVision.API.Controllers;

// Manager: agent sign-ups of their department. Owner: manager sign-ups.
[ApiController]
[Authorize(Policy = Policies.Reviewers)]
[Route("api/[controller]")]
public class SignupRequestsController : ControllerBase
{
    private readonly ISignupReviewService _reviews;

    public SignupRequestsController(ISignupReviewService reviews) => _reviews = reviews;

    [HttpGet]
    public async Task<ActionResult<List<SignupRequestItemDto>>> GetPending() => Ok(await _reviews.GetPendingAsync());

    [HttpPost("{userId:int}/approve")]
    public async Task<ActionResult<SignupRequestItemDto>> Approve(int userId, ApproveSignupDto dto)
        => Ok(await _reviews.ApproveAsync(userId, dto.Permissions));

    [HttpPost("{userId:int}/reject")]
    public async Task<ActionResult<SignupRequestItemDto>> Reject(int userId, RejectRequestDto dto)
        => Ok(await _reviews.RejectAsync(userId, dto.Reason));
}
