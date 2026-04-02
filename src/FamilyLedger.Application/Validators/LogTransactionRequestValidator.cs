using FamilyLedger.Application.DTOs;
using FluentValidation;

namespace FamilyLedger.Application.Validators;

public class LogTransactionRequestValidator : AbstractValidator<LogTransactionRequest>
{
    public LogTransactionRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Note).MaximumLength(1000).When(x => x.Note is not null);
        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Transaction date cannot be in the future.");
    }
}
