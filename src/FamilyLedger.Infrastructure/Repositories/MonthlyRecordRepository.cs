using FamilyLedger.Application.Interfaces;
using FamilyLedger.Domain.Entities;
using FamilyLedger.Domain.Enums;
using FamilyLedger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyLedger.Infrastructure.Repositories;

public sealed class MonthlyRecordRepository(AppDbContext db) : IMonthlyRecordRepository
{
    public async Task<MonthlyRecord?> GetOpenRecordAsync(Guid profileId, CancellationToken ct = default)
        => await db.MonthlyRecords.FirstOrDefaultAsync(x => x.ProfileId == profileId && x.Status == MonthlyRecordStatus.Open, ct);

    public async Task LinkTransactionAsync(Guid monthlyRecordId, Guid transactionId, CancellationToken ct = default)
        => await db.MonthlyTransactions.AddAsync(new MonthlyTransaction { MonthlyRecordId = monthlyRecordId, TransactionId = transactionId }, ct);
}
