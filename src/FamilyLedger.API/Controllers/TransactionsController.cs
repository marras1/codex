using System.Security.Claims;
using FamilyLedger.Application.DTOs;
using FamilyLedger.Application.Interfaces;
using FamilyLedger.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyLedger.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TransactionsController(ITransactionService transactionService) : ControllerBase
{
    private Guid ProfileId => Guid.Parse(User.FindFirstValue("profileId")!);
    private Guid MemberId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Authorize(Policy = "EditorOrOwner")]
    public async Task<IActionResult> LogTransaction([FromBody] LogTransactionRequest request, CancellationToken ct)
    {
        var result = await transactionService.LogTransactionAsync(ProfileId, MemberId, request, ct);
        return CreatedAtAction(nameof(GetTransaction), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTransaction(Guid id, CancellationToken ct)
    {
        var result = await transactionService.GetByIdAsync(ProfileId, id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] Guid? accountId, [FromQuery] TransactionCategory? category,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken ct = default)
        => Ok(await transactionService.GetTransactionsAsync(ProfileId, new TransactionQuery(ProfileId, accountId, category, from, to, limit, offset), ct));

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategoryBreakdown([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
        => Ok(await transactionService.GetCategoryBreakdownAsync(ProfileId, from, to, ct));
}
