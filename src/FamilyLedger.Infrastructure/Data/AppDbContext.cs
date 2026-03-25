using FamilyLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyLedger.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Allocation> Allocations => Set<Allocation>();
    public DbSet<AllocationSource> AllocationSources => Set<AllocationSource>();
    public DbSet<RecurringItem> RecurringItems => Set<RecurringItem>();
    public DbSet<RecurringEntry> RecurringEntries => Set<RecurringEntry>();
    public DbSet<MonthlyRecord> MonthlyRecords => Set<MonthlyRecord>();
    public DbSet<MonthlyTransaction> MonthlyTransactions => Set<MonthlyTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
