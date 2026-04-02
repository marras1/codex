namespace FamilyLedger.Domain.Enums;

public enum AccountType { Cash, Savings, Stocks, Etf, Retirement, Crypto, Property, Other }
public enum TransactionDirection { Debit, Credit }
public enum TransactionCategory
{
    Groceries, Dining, Transport, Fuel, Utilities, RentMortgage,
    LoanRepayment, Insurance, Healthcare, Education, Entertainment,
    Clothing, Electronics, Subscriptions, Travel, Gifts,
    Income, Transfer, Investment, Other
}
public enum AllocationStatus { Active, Paused, Completed }
public enum RecurringFrequency { Monthly, Quarterly, Annual }
public enum MonthlyRecordStatus { Open, Closing, Locked }
public enum MemberRole { Owner, Editor, Viewer }
