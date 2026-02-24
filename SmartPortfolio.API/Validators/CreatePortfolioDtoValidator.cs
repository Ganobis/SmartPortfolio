using FluentValidation;
using SmartPortfolio.API.Dtos.Portfolios;
using SmartPortfolio.Domain.ValueObjects;

public class CreatePortfolioDtoValidator : AbstractValidator<CreatePortfolioDto>
{
    public CreatePortfolioDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Portfolio name is required.")
            .MaximumLength(100).WithMessage("Portfolio name must not exceed 100 characters.");
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Must(currency => currency.Length == 3)
            .Must(currency => Money.AllowedCurrencies.Contains(currency.ToUpper()))
            .WithMessage($"Currency must be one of the following: {string.Join(", ", Money.AllowedCurrencies)}");
    }
}
