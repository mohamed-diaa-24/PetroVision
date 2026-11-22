using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PetroVision.API.Extensions;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;

namespace PetroVision.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    [EnableRateLimiting(RateLimits.Auth)]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
        => Ok(await _auth.LoginAsync(request));

    [HttpPost("signup")]
    [EnableRateLimiting(RateLimits.Auth)]
    public async Task<ActionResult<MessageResponseDto>> Signup(SignupRequestDto request)
        => Ok(await _auth.SignupAsync(request));

    [HttpPost("forgot-password")]
    [EnableRateLimiting(RateLimits.Auth)]
    public async Task<ActionResult<MessageResponseDto>> ForgotPassword(ForgotPasswordDto dto)
        => Ok(await _auth.ForgotPasswordAsync(dto));

    [HttpPost("reset-password")]
    [EnableRateLimiting(RateLimits.Auth)]
    public async Task<ActionResult<MessageResponseDto>> ResetPassword(ResetPasswordDto dto)
        => Ok(await _auth.ResetPasswordAsync(dto));

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me() => Ok(await _auth.GetProfileAsync(CurrentUserId()));

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateMe(UpdateProfileDto dto)
        => Ok(await _auth.UpdateProfileAsync(CurrentUserId(), dto));

    // Returns a new token: every other session is signed out.
    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<LoginResponseDto>> ChangePassword(ChangePasswordDto dto)
        => Ok(await _auth.ChangePasswordAsync(CurrentUserId(), dto));

    private int CurrentUserId() => int.Parse(User.FindFirst("sub")!.Value);
}
