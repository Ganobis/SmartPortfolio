using FluentAssertions;
using SmartPortfolio.Domain.ValueObjects;

namespace SmartPortfolio.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Should_Create_MoneyWith_Correct_Values()
    {
        var money = new Money(100, "PLN");
        money.Amount.Should().Be(100);
        money.Currency.Should().Be("PLN");
    }

    [Theory]
    [InlineData("currency")]
    [InlineData("")]
    public void Should_Throw_Exception_When_Currency_Is_Invalid(string currency)
    {
        Action act = () =>
        {
            var money = new Money(100, currency);
        };
        act.Should().Throw<ArgumentException>().WithMessage($"*{currency}*");
    }

    [Fact]
    public void Should_Add_Two_Amounts_Of_Same_Currency()
    {
        var money1 = new Money(100, "PLN");
        var money2 = new Money(10, "PLN");

        var result = money1 + money2;

        result.Amount.Should().Be(110);
        result.Currency.Should().Be("PLN");
    }

    [Fact]
    public void Should_Throw_Exception_When_Adding_Different_Currencies()
    {
        var money1 = new Money(100, "PLN");
        var money2 = new Money(10, "USD");
        Action act = () => { var result = money1 + money2; };
        act.Should().Throw<InvalidOperationException>().WithMessage("*currencies*");
    }

    [Fact]
    public void Should_Subtract_Two_Amounts_Of_Same_Currency()
    {
        var money1 = new Money(100, "PLN");
        var money2 = new Money(10, "PLN");
        var result = money1 - money2;
        result.Amount.Should().Be(90);
        result.Currency.Should().Be("PLN");
    }

    [Fact]
    public void Should_Throw_Exception_When_Subtract_Different_Currencies()
    {
        var money1 = new Money(100, "PLN");
        var money2 = new Money(10, "USD");
        Action act = () => { var result = money1 - money2; };
        act.Should().Throw<InvalidOperationException>().WithMessage("*currencies*");
    }

    [Fact]
    public void Should_Multiply_Money_Correctly()
    {
        var money1 = new Money(100, "PLN");
        var result = money1 * 2;
        result.Amount.Should().Be(200);
    }

    [Fact]
    public void Should_Compare_Money_Correctly()
    {
        var money1 = new Money(100, "PLN");
        var money2 = new Money(200, "PLN");

        (money2 > money1).Should().BeTrue();
        (money1 < money2).Should().BeTrue();
        (money2 >= money1).Should().BeTrue();
        (money1 <= money2).Should().BeTrue();
        (money1 == new Money(100, "PLN")).Should().BeTrue();
    }

    [Fact]
    public void Should_Throw_Exception_When_Compare_Less_Different_Currencies()
    {
        var money1 = new Money(100, "PLN");
        var money2 = new Money(10, "USD");
        Action act = () => { var result = money1 < money2; };
        act.Should().Throw<InvalidOperationException>().WithMessage("*currencies*");
    }

    [Fact]
    public void Should_Throw_Exception_When_Compare_Less_Or_Equal_Different_Currencies()
    {
        var money1 = new Money(100, "PLN");
        var money2 = new Money(10, "USD");
        Action act = () => { var result = money1 <= money2; };
        act.Should().Throw<InvalidOperationException>().WithMessage("*currencies*");
    }

    [Fact]
    public void Should_Throw_Exception_When_Compare_More_Or_Equal_Different_Currencies()
    {
        var money1 = new Money(100, "PLN");
        var money2 = new Money(10, "USD");
        Action act = () => { var result = money1 >= money2; };
        act.Should().Throw<InvalidOperationException>().WithMessage("*currencies*");
    }

}
