using FamilyLedger.Application.Interfaces;
using FamilyLedger.Domain.Entities;
using FamilyLedger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyLedger.Infrastructure.Repositories;

public sealed class MembershipRepository(AppDbContext db) : IMembershipRepository
{
    public async Task<Member?> GetAsync(Guid profileId, Guid userId, CancellationToken ct = default)
        => await db.Members.FirstOrDefaultAsync(x => x.ProfileId == profileId && x.UserId == userId, ct);

    public async Task<IReadOnlyList<Member>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await db.Members.Where(x => x.UserId == userId).ToListAsync(ct);

    public async Task<int> CountAsync(CancellationToken ct = default)
        => await db.Members.CountAsync(ct);

    public async Task AddAsync(Member member, CancellationToken ct = default)
        => await db.Members.AddAsync(member, ct);

    public Task RemoveAsync(Member member, CancellationToken ct = default)
    {
        db.Members.Remove(member);
        return Task.CompletedTask;
    }
}
