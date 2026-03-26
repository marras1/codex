using FamilyLedger.Application.Interfaces;
using FamilyLedger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyLedger.Infrastructure.Repositories;

public sealed class AdminStatsRepository(AppDbContext db) : IAdminStatsRepository
{
    public Task<int> CountUsersAsync(CancellationToken ct = default) => db.Users.CountAsync(ct);
    public Task<int> CountProfilesAsync(CancellationToken ct = default) => db.Profiles.CountAsync(ct);
    public Task<int> CountMembershipsAsync(CancellationToken ct = default) => db.Members.CountAsync(ct);
    public Task<int> CountTransactionsAsync(CancellationToken ct = default) => db.Transactions.CountAsync(ct);
}
