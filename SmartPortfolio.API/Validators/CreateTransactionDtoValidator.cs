using FluentValidation;
using SmartPortfolio.API.Dtos;
using SmartPortfolio.Domain.Entities;

namespace SmartPortfolio.API.Validators;

public class CreateTransactionDtoValidator : AbstractValidator<TransactionDto>
{
    public CreateTransactionDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transaction amount must be greater than zero.");
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Must(currency => currency.Length == 3)
            .Must(currency => Money.AllowedCurrencies.Contains(currency.ToUpper()))
            .WithMessage($"Currency must be one of the following: {string.Join(", ", Money.AllowedCurrencies)}");
    }
}
