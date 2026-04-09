using System.Security.Claims;
using FamilyLedger.Application.DTOs;
using FamilyLedger.Domain.Entities;
using FamilyLedger.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyLedger.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AccountsController(AppDbContext db) : ControllerBase
{
    private Guid ProfileId => Guid.Parse(User.FindFirstValue("profileId")!);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await db.Accounts
            .Where(x => x.ProfileId == ProfileId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => ToResponse(x))
            .ToListAsync(ct));

    [HttpPost]
    [Authorize(Policy = "EditorOrOwner")]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest request, CancellationToken ct)
    {
        var account = new Account
        {
            ProfileId = ProfileId,
            Name = request.Name,
            Type = request.Type,
            Institution = request.Institution,
            BalanceOverride = request.BalanceOverride,
            Currency = request.Currency
        };

        await db.Accounts.AddAsync(account, ct);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = account.Id }, ToResponse(account));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(x => x.Id == id && x.ProfileId == ProfileId, ct);
        return account is null ? NotFound() : Ok(ToResponse(account));
    }

    private static AccountResponse ToResponse(Account account)
        => new(account.Id, account.ProfileId, account.Name, account.Type, account.Institution, account.BalanceOverride, account.Currency, account.IsActive, account.CreatedAt);
}
