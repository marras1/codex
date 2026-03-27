using FamilyLedger.Application.DTOs;
using FamilyLedger.Domain.Entities;

namespace FamilyLedger.Application.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponse> LogTransactionAsync(Guid profileId, Guid memberId, LogTransactionRequest request, CancellationToken ct = default);
    Task<PagedResult<TransactionResponse>> GetTransactionsAsync(Guid profileId, TransactionQuery query, CancellationToken ct = default);
    Task<TransactionResponse?> GetByIdAsync(Guid profileId, Guid id, CancellationToken ct = default);
    Task<CategoryBreakdownResponse> GetCategoryBreakdownAsync(Guid profileId, DateOnly from, DateOnly to, CancellationToken ct = default);
}

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Transaction>> GetPagedAsync(TransactionQuery query, CancellationToken ct = default);
    Task AddAsync(Transaction transaction, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> GetRangeAsync(Guid profileId, DateOnly from, DateOnly to, CancellationToken ct = default);
}

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
}

public interface IMonthlyRecordRepository
{
    Task<MonthlyRecord?> GetOpenRecordAsync(Guid profileId, CancellationToken ct = default);
    Task LinkTransactionAsync(Guid monthlyRecordId, Guid transactionId, CancellationToken ct = default);
}
