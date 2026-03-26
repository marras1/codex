using System.Security.Claims;
using FamilyLedger.Application.DTOs;
using FamilyLedger.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyLedger.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("register-user")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
        => Ok(await authService.RegisterAsync(request, ct));

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        => Ok(await authService.LoginAsync(request, ct));

    [HttpGet("profiles")]
    [Authorize]
    public async Task<IActionResult> MyProfiles(CancellationToken ct)
        => Ok(await authService.GetMyProfilesAsync(UserId, ct));

    [HttpPost("profiles")]
    [Authorize]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileRequest request, CancellationToken ct)
        => Ok(await authService.CreateProfileForCurrentUserAsync(UserId, request, ct));

    [HttpPost("switch-profile/{profileId:guid}")]
    [Authorize]
    public async Task<IActionResult> SwitchProfile(Guid profileId, CancellationToken ct)
        => Ok(await authService.SwitchProfileAsync(UserId, profileId, ct));
}
