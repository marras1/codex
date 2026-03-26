namespace FamilyLedger.Domain.Exceptions;

public abstract class DomainException(string message) : Exception(message);
public sealed class AccountNotFoundException(Guid id) : DomainException($"Account {id} not found.");
public sealed class MonthlyRecordLockedException(Guid id) : DomainException($"Monthly record {id} is locked.");
public sealed class UnauthorisedProfileAccessException() : DomainException("Access denied to this profile.");
public sealed class MonthAlreadyOpenException() : DomainException("A monthly record is already open.");
public sealed class PreviousMonthNotLockedException() : DomainException("Previous month must be locked first.");
