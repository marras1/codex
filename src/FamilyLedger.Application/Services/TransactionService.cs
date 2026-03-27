using AutoMapper;
using FamilyLedger.Application.DTOs;
using FamilyLedger.Application.Interfaces;
using FamilyLedger.Domain.Entities;
using FamilyLedger.Domain.Exceptions;

namespace FamilyLedger.Application.Services;

public sealed class TransactionService(
    ITransactionRepository transactionRepo,
    IAccountRepository accountRepo,
    IMonthlyRecordRepository monthlyRecordRepo,
    IMapper mapper) : ITransactionService
{
    public async Task<TransactionResponse> LogTransactionAsync(Guid profileId, Guid memberId, LogTransactionRequest request, CancellationToken ct = default)
    {
        var account = await accountRepo.GetByIdAsync(request.AccountId, ct)
            ?? throw new AccountNotFoundException(request.AccountId);

        if (account.ProfileId != profileId)
            throw new UnauthorisedProfileAccessException();

        var transaction = Transaction.Create(request.AccountId, memberId, request.Amount, request.Direction,
            request.Description, request.Date, request.Category, request.Note);

        await transactionRepo.AddAsync(transaction, ct);

        var record = await monthlyRecordRepo.GetOpenRecordAsync(profileId, ct);
        if (record is not null && (short)request.Date.Month == record.Month && request.Date.Year == record.Year)
            await monthlyRecordRepo.LinkTransactionAsync(record.Id, transaction.Id, ct);

        await transactionRepo.SaveChangesAsync(ct);
        return mapper.Map<TransactionResponse>(transaction);
    }

    public async Task<PagedResult<TransactionResponse>> GetTransactionsAsync(Guid profileId, TransactionQuery query, CancellationToken ct = default)
    {
        var result = await transactionRepo.GetPagedAsync(query with { ProfileId = profileId }, ct);
        return new PagedResult<TransactionResponse>(result.Data.Select(mapper.Map<TransactionResponse>).ToList(), result.Total, result.Offset, result.Limit);
    }

    public async Task<TransactionResponse?> GetByIdAsync(Guid profileId, Guid id, CancellationToken ct = default)
    {
        var transaction = await transactionRepo.GetByIdAsync(id, ct);
        if (transaction is null) return null;
        var account = await accountRepo.GetByIdAsync(transaction.AccountId, ct);
        if (account?.ProfileId != profileId) throw new UnauthorisedProfileAccessException();
        return mapper.Map<TransactionResponse>(transaction);
    }

    public async Task<CategoryBreakdownResponse> GetCategoryBreakdownAsync(Guid profileId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var data = await transactionRepo.GetRangeAsync(profileId, from, to, ct);
        var grouped = data.Where(t => t.Direction == Domain.Enums.TransactionDirection.Debit)
            .GroupBy(t => t.Category)
            .Select(g => new CategoryBreakdownItem(g.Key, g.Sum(x => x.Amount), g.Count()))
            .OrderByDescending(x => x.Total)
            .ToList();

        return new CategoryBreakdownResponse(from, to, grouped, grouped.Sum(x => x.Total));
    }
}
