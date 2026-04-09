using AutoMapper;
using FamilyLedger.Application.DTOs;
using FamilyLedger.Application.Interfaces;
using FamilyLedger.Application.Mappings;
using FamilyLedger.Application.Services;
using FamilyLedger.Domain.Entities;
using FamilyLedger.Domain.Enums;
using FamilyLedger.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace FamilyLedger.UnitTests;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepo = new();
    private readonly Mock<IAccountRepository> _accountRepo = new();
    private readonly Mock<IMonthlyRecordRepository> _monthlyRecordRepo = new();
    private readonly TransactionService _sut;

    public TransactionServiceTests()
    {
        var mapper = new MapperConfiguration(c => c.AddProfile<MappingProfile>(), null).CreateMapper();
        _sut = new TransactionService(_transactionRepo.Object, _accountRepo.Object, _monthlyRecordRepo.Object, mapper);
    }

    [Fact]
    public async Task LogTransaction_ValidRequest_ReturnsCreated()
    {
        var profileId = Guid.NewGuid();
        var account = new Account { Id = Guid.NewGuid(), ProfileId = profileId, Name = "Test", Type = AccountType.Savings, Currency = "EUR" };

        _accountRepo.Setup(r => r.GetByIdAsync(account.Id, default)).ReturnsAsync(account);
        _monthlyRecordRepo.Setup(r => r.GetOpenRecordAsync(profileId, default)).ReturnsAsync((MonthlyRecord?)null);

        var request = new LogTransactionRequest(account.Id, 50m, TransactionDirection.Debit, "Test", DateOnly.FromDateTime(DateTime.UtcNow), TransactionCategory.Groceries);

        var result = await _sut.LogTransactionAsync(profileId, Guid.NewGuid(), request);

        result.Amount.Should().Be(50m);
        _transactionRepo.Verify(r => r.AddAsync(It.IsAny<Transaction>(), default), Times.Once);
    }

    [Fact]
    public async Task LogTransaction_WrongProfile_ThrowsUnauthorised()
    {
        var account = new Account { Id = Guid.NewGuid(), ProfileId = Guid.NewGuid(), Name = "Other", Type = AccountType.Cash, Currency = "EUR" };
        _accountRepo.Setup(r => r.GetByIdAsync(account.Id, default)).ReturnsAsync(account);

        await Assert.ThrowsAsync<UnauthorisedProfileAccessException>(() => _sut.LogTransactionAsync(Guid.NewGuid(), Guid.NewGuid(),
            new LogTransactionRequest(account.Id, 50m, TransactionDirection.Debit, "Test", DateOnly.FromDateTime(DateTime.UtcNow), TransactionCategory.Other)));
    }
}
