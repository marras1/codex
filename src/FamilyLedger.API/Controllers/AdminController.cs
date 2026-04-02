using FamilyLedger.Application.DTOs;
using FamilyLedger.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyLedger.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = "SuperAdminOnly")]
public class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
        => Ok(await adminService.GetDashboardAsync(ct));

    [HttpPost("profiles")]
    public async Task<IActionResult> CreateProfile([FromBody] AdminCreateProfileRequest request, CancellationToken ct)
        => Ok(await adminService.CreateProfileAsync(request, ct));

    [HttpPut("profiles/{profileId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> AssignMember(Guid profileId, Guid userId, [FromBody] AdminAssignMemberRequest request, CancellationToken ct)
    {
        await adminService.AssignMemberAsync(profileId, userId, request.Role, ct);
        return NoContent();
    }

    [HttpDelete("profiles/{profileId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RevokeMember(Guid profileId, Guid userId, CancellationToken ct)
    {
        await adminService.RevokeMemberAsync(profileId, userId, ct);
        return NoContent();
    }
}
