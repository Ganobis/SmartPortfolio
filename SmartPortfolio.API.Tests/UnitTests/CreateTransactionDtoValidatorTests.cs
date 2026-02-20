using FluentValidation.TestHelper;
using SmartPortfolio.API.Dtos;
using SmartPortfolio.API.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SmartPortfolio.API.Tests.UnitTests;

public class CreateTransactionDtoValidatorTests
{
    private readonly CreateTransactionDtoValidator _validator;

    public CreateTransactionDtoValidatorTests()
    {
        _validator = new CreateTransactionDtoValidator();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Data_Is_Valid()
    {
        var model = new CreateTransactionDto(100, "USD");

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Should_Have_Error_When_Amount_Is_Zero_Or_Less(decimal invalidAmount)
    {
        var model = new CreateTransactionDto(invalidAmount, "PLN");

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Amount)
              .WithErrorMessage("Transaction amount must be greater than zero.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDA")]
    [InlineData("XYZ")]
    public void Should_Have_Error_When_Currency_Is_Invalid(string invalidCurrency)
    {
        var model = new CreateTransactionDto(100, invalidCurrency);

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }
}