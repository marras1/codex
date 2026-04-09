using FamilyLedger.Domain.Enums;

namespace FamilyLedger.Application.DTOs;

public record MemberSummary(Guid Id, string DisplayName);
public record CreateAccountRequest(
    string Name,
    AccountType Type,
    string Currency,
    decimal? BalanceOverride,
    string? Institution = null);
public record AccountResponse(
    Guid Id,
    Guid ProfileId,
    string Name,
    AccountType Type,
    string? Institution,
    decimal? BalanceOverride,
    string Currency,
    bool IsActive,
    DateTime CreatedAt);
public record LogTransactionRequest(
    Guid AccountId,
    decimal Amount,
    TransactionDirection Direction,
    string Description,
    DateOnly Date,
    TransactionCategory Category,
    string? Note = null);
public record TransactionResponse(
    Guid Id,
    Guid AccountId,
    MemberSummary LoggedBy,
    decimal Amount,
    TransactionDirection Direction,
    string Description,
    string? Note,
    DateOnly Date,
    TransactionCategory Category,
    DateTime CreatedAt);
public record PagedResult<T>(IReadOnlyList<T> Data, int Total, int Offset, int Limit);
public record TransactionQuery(Guid ProfileId, Guid? AccountId, TransactionCategory? Category, DateOnly? From, DateOnly? To, int Limit = 50, int Offset = 0);
public record CategoryBreakdownItem(TransactionCategory Category, decimal Total, int Count);
public record CategoryBreakdownResponse(DateOnly From, DateOnly To, IReadOnlyList<CategoryBreakdownItem> Breakdown, decimal GrandTotal);
