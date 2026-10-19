using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
        => Ok(await _auth.LoginAsync(request));

    [HttpPost("signup")]
    public async Task<ActionResult<MessageResponseDto>> Signup(SignupRequestDto request)
        => Ok(await _auth.SignupAsync(request));

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var id = int.Parse(User.FindFirst("sub")!.Value);
        return Ok(await _auth.GetProfileAsync(id));
    }
}
