using FluentAssertions;
using SmartPortfolio.Domain.Entities;
using SmartPortfolio.Domain.ValueObjects;

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
    public void Should_Record_Transaction_On_Deposit()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");

        portfolio.Deposit(new Money(250, "PLN"));

        portfolio.Transactions.Should().HaveCount(1);
        portfolio.Transactions.First().Amount.Should().Be(250);
        portfolio.Transactions.First().Currency.Should().Be("PLN");
    }

    [Fact]
    public void Should_Record_Negative_Transaction_On_Withdraw()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        portfolio.Deposit(new Money(500, "PLN"));

        portfolio.Withdraw(new Money(200, "PLN"));

        portfolio.Transactions.Should().HaveCount(2);
        portfolio.Transactions.Last().Amount.Should().Be(-200);
    }

    [Fact]
    public void Should_Throw_Exception_When_Depositing_Zero()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");

        Action act = () => portfolio.Deposit(new Money(0, "PLN"));

        act.Should().Throw<ArgumentException>().WithMessage("*positive*");
    }

    [Fact]
    public void Should_Throw_Exception_When_Withdrawing_Zero()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        portfolio.Deposit(new Money(100, "PLN"));

        Action act = () => portfolio.Withdraw(new Money(0, "PLN"));

        act.Should().Throw<ArgumentException>().WithMessage("*positive*");
    }

    [Fact]
    public void Should_Buy_Asset_And_Reduce_Balance()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        portfolio.Deposit(new Money(5000, "PLN"));

        portfolio.BuyAsset("AAPL", 10, 150.00m);

        portfolio.Assets.Should().HaveCount(1);
        portfolio.Assets.First().Ticker.Should().Be("AAPL");
        portfolio.Assets.First().TotalQuantity.Should().Be(10);
        portfolio.Balance.Amount.Should().Be(3500);
    }

    [Fact]
    public void Should_Add_Lot_To_Existing_Asset_On_Second_Buy()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        portfolio.Deposit(new Money(10000, "PLN"));

        portfolio.BuyAsset("AAPL", 5, 150.00m);
        portfolio.BuyAsset("AAPL", 3, 160.00m);

        portfolio.Assets.Should().HaveCount(1);
        portfolio.Assets.First().TotalQuantity.Should().Be(8);
        portfolio.Assets.First().AssetLots.Should().HaveCount(2);
    }

    [Fact]
    public void Should_Create_Separate_Assets_For_Different_Tickers()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        portfolio.Deposit(new Money(10000, "PLN"));

        portfolio.BuyAsset("AAPL", 5, 150.00m);
        portfolio.BuyAsset("MSFT", 3, 300.00m);

        portfolio.Assets.Should().HaveCount(2);
    }

    [Fact]
    public void Should_Throw_When_Buying_Asset_With_Insufficient_Funds()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        portfolio.Deposit(new Money(100, "PLN"));

        Action act = () => portfolio.BuyAsset("AAPL", 10, 150.00m);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Insufficient funds*");
    }

    [Fact]
    public void Should_Record_Withdraw_Transaction_On_Buy()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        portfolio.Deposit(new Money(5000, "PLN"));

        portfolio.BuyAsset("AAPL", 10, 100.00m);

        portfolio.Transactions.Should().HaveCount(2);
        portfolio.Transactions.Last().Amount.Should().Be(-1000);
    }

    [Fact]
    public void Should_Match_Existing_Asset_Case_Insensitively()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        portfolio.Deposit(new Money(10000, "PLN"));

        portfolio.BuyAsset("aapl", 5, 150.00m);
        portfolio.BuyAsset("AAPL", 3, 160.00m);

        portfolio.Assets.Should().HaveCount(1);
        portfolio.Assets.First().TotalQuantity.Should().Be(8);
    }

    [Fact]
    public void Should_Pass_Transaction_Date_To_Asset_Lot()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        portfolio.Deposit(new Money(5000, "PLN"));
        var date = new DateTime(2025, 6, 15);

        portfolio.BuyAsset("AAPL", 5, 150.00m, date);

        portfolio.Assets.First().AssetLots.First().PurchaseDate.Should().Be(date);
    }


    [Fact]
    public void Should_Sell_Asset_And_Increase_Balance()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        portfolio.Deposit(new Money(5000, "PLN"));
        portfolio.BuyAsset("AAPL", 10, 150.00m);

        portfolio.SellAsset("AAPL", 5, 200.00m);

        portfolio.Assets.First().TotalQuantity.Should().Be(5);
        portfolio.Balance.Amount.Should().Be(4500);
    }

    [Fact]
    public void Should_Remove_Asset_When_All_Shares_Sold()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        portfolio.Deposit(new Money(5000, "PLN"));
        portfolio.BuyAsset("AAPL", 10, 150.00m);

        portfolio.SellAsset("AAPL", 10, 200.00m);

        portfolio.Assets.Should().BeEmpty();
    }

    [Fact]
    public void Should_Record_Deposit_Transaction_On_Sell()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        portfolio.Deposit(new Money(5000, "PLN"));
        portfolio.BuyAsset("AAPL", 10, 100.00m);

        portfolio.SellAsset("AAPL", 5, 120.00m);

        portfolio.Transactions.Should().HaveCount(3);
        portfolio.Transactions.Last().Amount.Should().Be(600);
    }

    [Fact]
    public void Should_Throw_When_Selling_Ticker_Not_Owned()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        portfolio.Deposit(new Money(5000, "PLN"));

        Action act = () => portfolio.SellAsset("AAPL", 5, 150.00m);

        act.Should().Throw<InvalidOperationException>().WithMessage("*don't own enough shares*");
    }

    [Fact]
    public void Should_Throw_When_Selling_More_Shares_Than_Owned()
    {
        var portfolio = new Portfolio("My Portfolio", Guid.NewGuid(), "PLN");
        portfolio.Deposit(new Money(5000, "PLN"));
        portfolio.BuyAsset("AAPL", 5, 150.00m);

        Action act = () => portfolio.SellAsset("AAPL", 10, 200.00m);

        act.Should().Throw<InvalidOperationException>().WithMessage("*don't own enough shares*");
    }
}