using FluentAssertions;
using SmartPortfolio.Domain.Entities;

namespace SmartPortfolio.API.Tests.UnitTests;
public class AssetLotTests
{
    private readonly Guid _assetId = Guid.NewGuid();

    [Fact]
    public void Constructor_Should_Create_Lot_With_Valid_Data()
    {
        var lot = new AssetLot(_assetId, 10, 150.00m);

        lot.Id.Should().NotBeEmpty();
        lot.AssetId.Should().Be(_assetId);
        lot.Quantity.Should().Be(10);
        lot.PricePerShare.Should().Be(150.00m);
        lot.PurchaseDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_Should_Use_Provided_PurchaseDate()
    {
        var date = new DateTime(2025, 6, 15);

        var lot = new AssetLot(_assetId, 5, 100.00m, date);

        lot.PurchaseDate.Should().Be(date);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Should_Throw_When_Quantity_Is_Zero_Or_Negative(decimal invalidQuantity)
    {
        Action act = () => new AssetLot(_assetId, invalidQuantity, 100.00m);

        act.Should().Throw<ArgumentException>()
           .WithMessage("Quantity must be greater than zero.");
    }

    [Fact]
    public void Constructor_Should_Throw_When_Price_Is_Negative()
    {
        Action act = () => new AssetLot(_assetId, 10, -5.00m);

        act.Should().Throw<ArgumentException>()
           .WithMessage("Price cannot be negative.");
    }

    [Fact]
    public void Constructor_Should_Allow_Zero_Price()
    {
        var lot = new AssetLot(_assetId, 10, 0m);

        lot.PricePerShare.Should().Be(0);
    }

    [Fact]
    public void ReduceQuantity_Should_Decrease_Quantity()
    {
        var lot = new AssetLot(_assetId, 10, 150.00m);

        lot.ReduceQuantity(3);

        lot.Quantity.Should().Be(7);
    }

    [Fact]
    public void ReduceQuantity_Should_Allow_Reducing_To_Zero()
    {
        var lot = new AssetLot(_assetId, 10, 150.00m);

        lot.ReduceQuantity(10);

        lot.Quantity.Should().Be(0);
    }

    [Fact]
    public void ReduceQuantity_Should_Throw_When_Amount_Exceeds_Quantity()
    {
        var lot = new AssetLot(_assetId, 5, 100.00m);

        Action act = () => lot.ReduceQuantity(10);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Cannot reduce more than the lot contains.");
    }
}