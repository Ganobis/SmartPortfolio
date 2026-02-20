using FluentValidation;
using SmartPortfolio.API.Dtos;

namespace SmartPortfolio.API.Validators;

public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email address is invalid");

        RuleFor(x => x.Name)
            .NotEmpty().Length(3, 50);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password is too short");
    }
}