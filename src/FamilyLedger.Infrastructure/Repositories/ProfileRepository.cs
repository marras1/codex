using FamilyLedger.Application.Interfaces;
using FamilyLedger.Domain.Entities;
using FamilyLedger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyLedger.Infrastructure.Repositories;

public sealed class ProfileRepository(AppDbContext db) : IProfileRepository
{
    public async Task<Profile?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Profiles.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Profile profile, CancellationToken ct = default)
        => await db.Profiles.AddAsync(profile, ct);

    public async Task<int> CountAsync(CancellationToken ct = default)
        => await db.Profiles.CountAsync(ct);
}
