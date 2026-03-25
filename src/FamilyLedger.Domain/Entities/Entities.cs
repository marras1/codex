using FamilyLedger.Domain.Enums;

namespace FamilyLedger.Domain.Entities;

public class Profile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = "EUR";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Member
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfileId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public MemberRole Role { get; set; } = MemberRole.Editor;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string? Institution { get; set; }
    public decimal? BalanceOverride { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateTime? SyncedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Transaction> Transactions { get; set; } = [];
}

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public Guid? LoggedBy { get; set; }
    public decimal Amount { get; set; }
    public TransactionDirection Direction { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateOnly Date { get; set; }
    public TransactionCategory Category { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static Transaction Create(Guid accountId, Guid memberId, decimal amount, TransactionDirection direction,
        string description, DateOnly date, TransactionCategory category, string? note = null)
        => new()
        {
            AccountId = accountId,
            LoggedBy = memberId,
            Amount = amount,
            Direction = direction,
            Description = description,
            Date = date,
            Category = category,
            Note = note
        };
}

public class Allocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public AllocationStatus Status { get; set; } = AllocationStatus.Active;
    public DateOnly? TargetDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AllocationSource
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AllocationId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
}

public class RecurringItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfileId { get; set; }
    public Guid? AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TransactionCategory Category { get; set; } = TransactionCategory.Other;
    public decimal ExpectedAmount { get; set; }
    public RecurringFrequency Frequency { get; set; } = RecurringFrequency.Monthly;
    public short DayOfMonth { get; set; } = 1;
    public bool AutoLog { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class MonthlyRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfileId { get; set; }
    public short Month { get; set; }
    public short Year { get; set; }
    public decimal IncomeTotal { get; set; }
    public decimal SpendingTotal { get; set; }
    public decimal RecurringTotal { get; set; }
    public decimal AllocationTotal { get; set; }
    public decimal Leftover { get; set; }
    public MonthlyRecordStatus Status { get; set; } = MonthlyRecordStatus.Open;
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
}

public class MonthlyTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MonthlyRecordId { get; set; }
    public Guid TransactionId { get; set; }
}

public class RecurringEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MonthlyRecordId { get; set; }
    public Guid RecurringItemId { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal? ActualAmount { get; set; }
    public bool Confirmed { get; set; }
    public DateOnly? PaidDate { get; set; }
    public Guid? TransactionId { get; set; }
}
