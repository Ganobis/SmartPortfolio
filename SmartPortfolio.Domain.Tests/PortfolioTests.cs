using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using SmartPortfolio.Domain.Entities;

namespace SmartPortfolio.Domain.Tests;

public class PortfolioTests
{
    [Fact]
    public void Should_Initialize_With_Zero_Balance()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");

        portfolio.Balance.Amount.Should().Be(0);
        portfolio.Balance.Currency.Should().Be("PLN");
    }

    [Fact]
    public void Should_Deposit_Correct_Amount()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        var depositAmount = new Money(500, "PLN");

        portfolio.Deposit(depositAmount);

        portfolio.Balance.Amount.Should().Be(500);
    }

    [Fact]
    public void Should_Throw_Exception_When_Depositing_Negative_Amount()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        var depositAmount = new Money(-100, "PLN");
        Action act = () => portfolio.Deposit(depositAmount);
        act.Should().Throw<ArgumentException>().WithMessage("*positive*");
    }

    [Fact]
    public void Should_Withdraw_Correct_Amount()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        var depositAmount = new Money(500, "PLN");
        portfolio.Deposit(depositAmount);
        var withdrawAmount = new Money(200, "PLN");
        portfolio.Withdraw(withdrawAmount);
        portfolio.Balance.Amount.Should().Be(300);
    }

    [Fact]
    public void Should_Throw_Exception_When_Insufficient_Funds()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        var depositAmount = new Money(100, "PLN");
        portfolio.Deposit(depositAmount);
        var withdrawAmount = new Money(200, "PLN");
        Action act = () => portfolio.Withdraw(withdrawAmount);
        act.Should().Throw<InvalidOperationException>().WithMessage("*Insufficient funds*");
    }

    [Fact]
    public void Should_Throw_Exception_When_Withdraw_Negative_Amount()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        var withdrawAmount = new Money(-50, "PLN");
        Action act = () => portfolio.Withdraw(withdrawAmount);
        act.Should().Throw<ArgumentException>().WithMessage("*positive*");
    }

    [Fact]
    public void Should_Throw_Exception_When_Withdraw_Different_Currency()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        var depositAmount = new Money(500, "PLN");
        portfolio.Deposit(depositAmount);
        var withdrawAmount = new Money(100, "USD");
        Action act = () => portfolio.Withdraw(withdrawAmount);
        act.Should().Throw<InvalidOperationException>().WithMessage("*currencies*");
    }

    [Fact]
    public void Should_Throw_Exception_When_Deposit_Different_Currency()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        var depositAmount = new Money(100, "USD");
        Action act = () => portfolio.Deposit(depositAmount);
        act.Should().Throw<InvalidOperationException>().WithMessage("*currencies*");
    }

    [Fact]
    public void Should_Create_Portfolio_With_Invalid_Name()
    {
        Action act = () => new Portfolio("", Guid.NewGuid(), "PLN");
        act.Should().Throw<ArgumentException>().WithMessage("*name cannot be empty*");
    }

    [Fact]
    public void Should_Create_Portfolio_With_Empty_Constructor()
    {
        var portfolio = new Portfolio();
        portfolio.Id.Should().Be(Guid.Empty);
    }
}