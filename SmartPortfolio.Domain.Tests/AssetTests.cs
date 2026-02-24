using FluentAssertions;
using SmartPortfolio.Domain.Entities;

namespace SmartPortfolio.API.Tests.UnitTests;
public class AssetTests
{
    private readonly Guid _portfolioId = Guid.NewGuid();

    [Fact]
    public void Constructor_Should_Create_Asset_With_Valid_Data()
    {
        var asset = new Asset(_portfolioId, "AAPL");

        asset.Id.Should().NotBeEmpty();
        asset.PortfolioId.Should().Be(_portfolioId);
        asset.Ticker.Should().Be("AAPL");
        asset.AssetLots.Should().BeEmpty();
        asset.TotalQuantity.Should().Be(0);
    }

    [Fact]
    public void Constructor_Should_Uppercase_Ticker()
    {
        var asset = new Asset(_portfolioId, "msft");

        asset.Ticker.Should().Be("MSFT");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Constructor_Should_Throw_When_Ticker_Is_Null_Or_Empty(string? ticker)
    {
        Action act = () => new Asset(_portfolioId, ticker!);

        act.Should().Throw<ArgumentException>()
           .WithMessage("Ticker cant be empty.");
    }

    [Fact]
    public void AddLot_Should_Add_Lot_And_Update_TotalQuantity()
    {
        var asset = new Asset(_portfolioId, "AAPL");

        asset.AddLot(10, 150.00m);

        asset.AssetLots.Should().HaveCount(1);
        asset.TotalQuantity.Should().Be(10);
    }

    [Fact]
    public void AddLot_Should_Accumulate_Multiple_Lots()
    {
        var asset = new Asset(_portfolioId, "AAPL");

        asset.AddLot(10, 150.00m);
        asset.AddLot(5, 160.00m);

        asset.AssetLots.Should().HaveCount(2);
        asset.TotalQuantity.Should().Be(15);
    }

    [Fact]
    public void RemoveQuantity_Should_Remove_From_Oldest_Lot_First()
    {
        var asset = new Asset(_portfolioId, "AAPL");
        asset.AddLot(10, 100.00m, new DateTime(2025, 1, 1));
        asset.AddLot(5, 200.00m, new DateTime(2025, 6, 1));

        asset.RemoveQuantity(10);

        asset.AssetLots.Should().HaveCount(1);
        asset.TotalQuantity.Should().Be(5);
        asset.AssetLots.First().PricePerShare.Should().Be(200.00m);
    }

    [Fact]
    public void RemoveQuantity_Should_Partially_Reduce_Lot()
    {
        var asset = new Asset(_portfolioId, "AAPL");
        asset.AddLot(10, 150.00m);

        asset.RemoveQuantity(3);

        asset.AssetLots.Should().HaveCount(1);
        asset.TotalQuantity.Should().Be(7);
    }

    [Fact]
    public void RemoveQuantity_Should_Span_Multiple_Lots()
    {
        var asset = new Asset(_portfolioId, "TSLA");
        asset.AddLot(5, 100.00m, new DateTime(2025, 1, 1));
        asset.AddLot(5, 200.00m, new DateTime(2025, 6, 1));
        asset.AddLot(5, 300.00m, new DateTime(2025, 12, 1));

        asset.RemoveQuantity(8);

        asset.AssetLots.Should().HaveCount(2);
        asset.TotalQuantity.Should().Be(7);
    }

    [Fact]
    public void RemoveQuantity_Should_Remove_All_Lots_When_Selling_Everything()
    {
        var asset = new Asset(_portfolioId, "AAPL");
        asset.AddLot(5, 100.00m);
        asset.AddLot(5, 200.00m);

        asset.RemoveQuantity(10);

        asset.AssetLots.Should().BeEmpty();
        asset.TotalQuantity.Should().Be(0);
    }

    [Fact]
    public void RemoveQuantity_Should_Throw_When_Not_Enough_Shares()
    {
        var asset = new Asset(_portfolioId, "AAPL");
        asset.AddLot(5, 150.00m);

        Action act = () => asset.RemoveQuantity(10);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Not enough shares*");
    }
}