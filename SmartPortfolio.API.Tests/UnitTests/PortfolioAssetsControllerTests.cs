using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartPortfolio.API.Controllers;
using SmartPortfolio.API.Dtos.Assets;
using SmartPortfolio.API.Tests.UnitTests.Helpers;
using SmartPortfolio.Domain.Entities;
using SmartPortfolio.Domain.Interfaces;
using SmartPortfolio.Domain.ValueObjects;
using SmartPortfolio.Infrastructure.Persistence;
using SmartPortfolio.Infrastructure.Services.StockPrice;

namespace SmartPortfolio.API.Tests.UnitTests;
public class PortfolioAssetsControllerTests
{
    private readonly SmartPortfolioDbContext _context;
    private readonly Mock<IStockPricingService> _stockPricingServiceMock;
    private readonly PortfolioAssetsController _controller;
    private readonly Guid _ownerId = Guid.NewGuid();

    public PortfolioAssetsControllerTests()
    {
        var options = new DbContextOptionsBuilder<SmartPortfolioDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new SmartPortfolioDbContext(options);

        _stockPricingServiceMock = new Mock<IStockPricingService>();

        _controller = new PortfolioAssetsController(_context, _stockPricingServiceMock.Object);
        _controller.MockCurrentUser(_ownerId);
    }

    private async Task<Portfolio> CreatePortfolioWithBalanceAsync(decimal balance)
    {
        var portfolio = new Portfolio("Test Portfolio", _ownerId, "USD");
        if (balance > 0)
        {
            portfolio.Deposit(new Money(balance, "USD"));
        }
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();
        return portfolio;
    }

    private PortfolioAssetsController CreateControllerWithFakeService()
    {
        var fakeService = new FakeStockPricingService();
        var controller = new PortfolioAssetsController(_context, fakeService);
        controller.MockCurrentUser(_ownerId);
        return controller;
    }

    #region GetAll

    [Fact]
    public async Task GetAll_Should_Return_Ok_With_Assets()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(10000);
        portfolio.BuyAsset("AAPL", 10, 150.00m);
        portfolio.BuyAsset("MSFT", 5, 300.00m);
        await _context.SaveChangesAsync();

        var result = await _controller.GetAll(portfolio.Id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var assets = okResult.Value.Should().BeAssignableTo<List<AssetDto>>().Subject;
        assets.Should().HaveCount(2);
        assets.Should().Contain(a => a.Ticker == "AAPL" && a.TotalQuantity == 10);
        assets.Should().Contain(a => a.Ticker == "MSFT" && a.TotalQuantity == 5);
    }

    [Fact]
    public async Task GetAll_Should_Return_Ok_With_Empty_List_When_No_Assets()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(1000);

        var result = await _controller.GetAll(portfolio.Id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var assets = okResult.Value.Should().BeAssignableTo<List<AssetDto>>().Subject;
        assets.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_Should_Return_NotFound_When_Portfolio_Missing()
    {
        var result = await _controller.GetAll(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAll_Should_Return_NotFound_When_Portfolio_Belongs_To_Other_User()
    {
        var otherOwnerId = Guid.NewGuid();
        var portfolio = new Portfolio("Other Portfolio", otherOwnerId, "USD");
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();

        var result = await _controller.GetAll(portfolio.Id);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Buy

    [Fact]
    public async Task Buy_Should_Return_NoContent_With_Explicit_Price()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(10000);
        var dto = new BuyAssetDto("AAPL", 5, 150.00m);

        var result = await _controller.Buy(portfolio.Id, dto);

        result.Should().BeOfType<NoContentResult>();

        var updated = await _context.Portfolios
            .Include(p => p.Assets).ThenInclude(a => a.AssetLots)
            .FirstAsync(p => p.Id == portfolio.Id);
        updated.Assets.Should().HaveCount(1);
        updated.Assets.First().Ticker.Should().Be("AAPL");
        updated.Assets.First().TotalQuantity.Should().Be(5);
    }

    [Fact]
    public async Task Buy_Should_Fetch_Price_From_Service_When_Not_Provided()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(10000);
        _stockPricingServiceMock
            .Setup(s => s.GetCurrentPriceAsync("AAPL"))
            .ReturnsAsync(175.00m);
        var dto = new BuyAssetDto("AAPL", 5);

        var result = await _controller.Buy(portfolio.Id, dto);

        result.Should().BeOfType<NoContentResult>();
        _stockPricingServiceMock.Verify(s => s.GetCurrentPriceAsync("AAPL"), Times.Once);
    }

    [Fact]
    public async Task Buy_Should_Not_Call_Service_When_Price_Provided()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(10000);
        var dto = new BuyAssetDto("AAPL", 5, 150.00m);

        await _controller.Buy(portfolio.Id, dto);

        _stockPricingServiceMock.Verify(s => s.GetCurrentPriceAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Buy_Should_Return_BadRequest_When_Insufficient_Funds()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(100);
        var dto = new BuyAssetDto("AAPL", 10, 150.00m);

        var result = await _controller.Buy(portfolio.Id, dto);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().BeOfType<string>()
            .Which.Should().Contain("Insufficient funds");
    }

    [Fact]
    public async Task Buy_Should_Return_NotFound_When_Portfolio_Missing()
    {
        var dto = new BuyAssetDto("AAPL", 5, 150.00m);

        var result = await _controller.Buy(Guid.NewGuid(), dto);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Buy_Should_Pass_PurchaseDate_To_Domain()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(10000);
        var date = new DateTime(2025, 6, 15);
        var dto = new BuyAssetDto("AAPL", 5, 150.00m, date);

        await _controller.Buy(portfolio.Id, dto);

        var updated = await _context.Portfolios
            .Include(p => p.Assets).ThenInclude(a => a.AssetLots)
            .FirstAsync(p => p.Id == portfolio.Id);
        updated.Assets.First().AssetLots.First().PurchaseDate.Should().Be(date);
    }

    #endregion

    #region Sell

    [Fact]
    public async Task Sell_Should_Return_NoContent_With_Explicit_Price()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(10000);
        portfolio.BuyAsset("AAPL", 10, 150.00m);
        await _context.SaveChangesAsync();
        var dto = new SellAssetDto("AAPL", 5, 200.00m);

        var result = await _controller.Sell(portfolio.Id, dto);

        result.Should().BeOfType<NoContentResult>();

        var updated = await _context.Portfolios
            .Include(p => p.Assets).ThenInclude(a => a.AssetLots)
            .FirstAsync(p => p.Id == portfolio.Id);
        updated.Assets.First().TotalQuantity.Should().Be(5);
    }

    [Fact]
    public async Task Sell_Should_Fetch_Price_From_Service_When_Not_Provided()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(10000);
        portfolio.BuyAsset("AAPL", 10, 150.00m);
        await _context.SaveChangesAsync();
        _stockPricingServiceMock
            .Setup(s => s.GetCurrentPriceAsync("AAPL"))
            .ReturnsAsync(200.00m);
        var dto = new SellAssetDto("AAPL", 5);

        var result = await _controller.Sell(portfolio.Id, dto);

        result.Should().BeOfType<NoContentResult>();
        _stockPricingServiceMock.Verify(s => s.GetCurrentPriceAsync("AAPL"), Times.Once);
    }

    [Fact]
    public async Task Sell_Should_Not_Call_Service_When_Price_Provided()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(10000);
        portfolio.BuyAsset("AAPL", 10, 150.00m);
        await _context.SaveChangesAsync();
        var dto = new SellAssetDto("AAPL", 5, 200.00m);

        await _controller.Sell(portfolio.Id, dto);

        _stockPricingServiceMock.Verify(s => s.GetCurrentPriceAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Sell_Should_Return_BadRequest_When_Not_Enough_Shares()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(10000);
        portfolio.BuyAsset("AAPL", 5, 150.00m);
        await _context.SaveChangesAsync();
        var dto = new SellAssetDto("AAPL", 20, 200.00m);

        var result = await _controller.Sell(portfolio.Id, dto);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().BeOfType<string>()
            .Which.Should().Contain("don't own enough shares");
    }

    [Fact]
    public async Task Sell_Should_Return_NotFound_When_Portfolio_Missing()
    {
        var dto = new SellAssetDto("AAPL", 5, 200.00m);

        var result = await _controller.Sell(Guid.NewGuid(), dto);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Sell_Should_Remove_Asset_When_All_Shares_Sold()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(10000);
        portfolio.BuyAsset("AAPL", 10, 150.00m);
        await _context.SaveChangesAsync();
        var dto = new SellAssetDto("AAPL", 10, 200.00m);

        await _controller.Sell(portfolio.Id, dto);

        var updated = await _context.Portfolios
            .Include(p => p.Assets)
            .FirstAsync(p => p.Id == portfolio.Id);
        updated.Assets.Should().BeEmpty();
    }

    #endregion

    #region FakeStockPricingService

    [Fact]
    public async Task Buy_WithFakeService_Should_Use_Known_Ticker_Price()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(10000);
        var fakeController = CreateControllerWithFakeService();
        // FakeStockPricingService returns 150.00 for AAPL
        var dto = new BuyAssetDto("AAPL", 5);

        var result = await fakeController.Buy(portfolio.Id, dto);

        result.Should().BeOfType<NoContentResult>();

        var updated = await _context.Portfolios
            .Include(p => p.Assets).ThenInclude(a => a.AssetLots)
            .FirstAsync(p => p.Id == portfolio.Id);
        updated.Assets.First().TotalQuantity.Should().Be(5);
        updated.Assets.First().AssetLots.First().PricePerShare.Should().Be(150.00m);
        // 10000 - (5 * 150) = 9250
        updated.Balance.Amount.Should().Be(9250);
    }

    [Fact]
    public async Task Sell_WithFakeService_Should_Use_Known_Ticker_Price()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(10000);
        portfolio.BuyAsset("MSFT", 10, 300.00m);
        await _context.SaveChangesAsync();
        var fakeController = CreateControllerWithFakeService();
        // FakeStockPricingService returns 300.00 for MSFT
        var dto = new SellAssetDto("MSFT", 4);

        var result = await fakeController.Sell(portfolio.Id, dto);

        result.Should().BeOfType<NoContentResult>();

        var updated = await _context.Portfolios
            .Include(p => p.Assets).ThenInclude(a => a.AssetLots)
            .FirstAsync(p => p.Id == portfolio.Id);
        updated.Assets.First().TotalQuantity.Should().Be(6);
        // 10000 - 3000 (buy) + 1200 (sell 4 * 300) = 8200
        updated.Balance.Amount.Should().Be(8200);
    }

    [Fact]
    public async Task Buy_WithFakeService_Should_Use_Deterministic_Price_For_Unknown_Ticker()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(50000);
        var fakeController = CreateControllerWithFakeService();
        var dto = new BuyAssetDto("XYZUNKNOWN", 2);

        var result = await fakeController.Buy(portfolio.Id, dto);

        result.Should().BeOfType<NoContentResult>();

        var updated = await _context.Portfolios
            .Include(p => p.Assets).ThenInclude(a => a.AssetLots)
            .FirstAsync(p => p.Id == portfolio.Id);
        var lot = updated.Assets.First().AssetLots.First();
        lot.PricePerShare.Should().BeGreaterThanOrEqualTo(10.50m);
        lot.PricePerShare.Should().BeLessThan(410.50m);
    }

    #endregion
}