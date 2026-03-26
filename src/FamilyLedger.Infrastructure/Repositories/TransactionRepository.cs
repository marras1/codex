using FamilyLedger.Application.DTOs;
using FamilyLedger.Application.Interfaces;
using FamilyLedger.Domain.Entities;
using FamilyLedger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyLedger.Infrastructure.Repositories;

public sealed class TransactionRepository(AppDbContext db) : ITransactionRepository
{
    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Transactions.FindAsync([id], ct);

    public async Task AddAsync(Transaction transaction, CancellationToken ct = default)
        => await db.Transactions.AddAsync(transaction, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);

    public async Task<PagedResult<Transaction>> GetPagedAsync(TransactionQuery q, CancellationToken ct = default)
    {
        var query = db.Transactions.Join(db.Accounts, t => t.AccountId, a => a.Id, (t, a) => new { t, a })
            .Where(x => x.a.ProfileId == q.ProfileId).Select(x => x.t);
        if (q.AccountId.HasValue) query = query.Where(t => t.AccountId == q.AccountId);
        if (q.Category.HasValue) query = query.Where(t => t.Category == q.Category);
        if (q.From.HasValue) query = query.Where(t => t.Date >= q.From.Value);
        if (q.To.HasValue) query = query.Where(t => t.Date <= q.To.Value);

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(t => t.Date).Skip(q.Offset).Take(q.Limit).ToListAsync(ct);
        return new PagedResult<Transaction>(items, total, q.Offset, q.Limit);
    }

    public async Task<IReadOnlyList<Transaction>> GetRangeAsync(Guid profileId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        return await db.Transactions.Join(db.Accounts, t => t.AccountId, a => a.Id, (t, a) => new { t, a })
            .Where(x => x.a.ProfileId == profileId && x.t.Date >= from && x.t.Date <= to)
            .Select(x => x.t)
            .ToListAsync(ct);
    }
}
