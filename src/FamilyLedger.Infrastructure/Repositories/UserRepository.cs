using FamilyLedger.Application.Interfaces;
using FamilyLedger.Domain.Entities;
using FamilyLedger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyLedger.Infrastructure.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await db.Users.AddAsync(user, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}
