using FamilyLedger.Application.DTOs;
using FamilyLedger.Application.Interfaces;
using FamilyLedger.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace FamilyLedger.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = "SuperAdminOnly")]
public class AdminController(IAdminService adminService, AppDbContext db, IHostEnvironment environment, IConfiguration configuration) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
        => Ok(await adminService.GetDashboardAsync(ct));

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken ct)
    {
        var dashboard = await adminService.GetDashboardAsync(ct);

        var users = await db.Users
            .OrderByDescending(x => x.CreatedAt)
            .Take(12)
            .Select(x => new
            {
                x.Id,
                x.DisplayName,
                x.Email,
                x.IsSuperAdmin,
                x.CreatedAt
            })
            .ToListAsync(ct);

        var profiles = await db.Profiles
            .OrderByDescending(x => x.CreatedAt)
            .Take(12)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Currency,
                x.CreatedAt,
                MemberCount = db.Members.Count(m => m.ProfileId == x.Id)
            })
            .ToListAsync(ct);

        var memberships = await db.Members
            .OrderByDescending(x => x.JoinedAt)
            .Take(16)
            .Join(db.Profiles,
                member => member.ProfileId,
                profile => profile.Id,
                (member, profile) => new
                {
                    member.Id,
                    member.UserId,
                    member.ProfileId,
                    ProfileName = profile.Name,
                    member.DisplayName,
                    member.Email,
                    member.Role,
                    member.JoinedAt
                })
            .ToListAsync(ct);

        var transactions = await db.Transactions
            .OrderByDescending(x => x.CreatedAt)
            .Take(16)
            .Join(db.Accounts,
                transaction => transaction.AccountId,
                account => account.Id,
                (transaction, account) => new
                {
                    transaction.Id,
                    transaction.AccountId,
                    AccountName = account.Name,
                    account.ProfileId,
                    transaction.Amount,
                    transaction.Direction,
                    transaction.Description,
                    transaction.Category,
                    transaction.Date,
                    transaction.CreatedAt
                })
            .Join(db.Profiles,
                item => item.ProfileId,
                profile => profile.Id,
                (item, profile) => new
                {
                    item.Id,
                    item.AccountId,
                    item.AccountName,
                    item.ProfileId,
                    ProfileName = profile.Name,
                    item.Amount,
                    item.Direction,
                    item.Description,
                    item.Category,
                    item.Date,
                    item.CreatedAt
                })
            .ToListAsync(ct);

        var settings = new
        {
            Environment = environment.EnvironmentName,
            SwaggerUrl = "/swagger",
            ApiBase = "/api/v1",
            Database = configuration.GetConnectionString("DefaultConnection") is null ? "Unavailable" : "Configured",
            AllowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? []
        };

        return Ok(new
        {
            dashboard,
            users,
            profiles,
            memberships,
            transactions,
            settings
        });
    }

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
