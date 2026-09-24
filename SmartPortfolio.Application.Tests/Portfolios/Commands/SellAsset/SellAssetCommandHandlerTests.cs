using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartPortfolio.Application.Portfolios.Commands.SellAsset;
using SmartPortfolio.Domain.Entities;
using SmartPortfolio.Domain.Interfaces;
using SmartPortfolio.Domain.ValueObjects;
using SmartPortfolio.Infrastructure.Persistence;

namespace SmartPortfolio.API.Tests.UnitTests;

public class SellAssetCommandHandlerTests
{
    private readonly SmartPortfolioDbContext _context;
    private readonly Mock<IStockPricingService> _stockPricingServiceMock;
    private readonly SellAssetCommandHandler _handler;
    private readonly Guid _ownerId = Guid.NewGuid();

    public SellAssetCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<SmartPortfolioDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new SmartPortfolioDbContext(options);

        _stockPricingServiceMock = new Mock<IStockPricingService>();

        _handler = new SellAssetCommandHandler(_context, _stockPricingServiceMock.Object);
    }

    private async Task<Portfolio> CreatePortfolioWithBalanceAndAssetAsync(decimal balance, string ticker, decimal quantity, decimal buyPrice)
    {
        var portfolio = new Portfolio("Test Portfolio", _ownerId, "USD");
        if (balance > 0)
        {
            portfolio.Deposit(new Money(balance, "USD"));
        }

        portfolio.BuyAsset(ticker, quantity, buyPrice);

        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();
        return portfolio;
    }

    [Fact]
    public async Task Handle_Should_Sell_Asset_With_Explicit_Price()
    {
        var portfolio = await CreatePortfolioWithBalanceAndAssetAsync(10000, "AAPL", 10, 150.00m);
        var command = new SellAssetCommand(portfolio.Id, _ownerId, "AAPL", 5, 200.00m);

        await _handler.Handle(command, CancellationToken.None);

        var updated = await _context.Portfolios
            .Include(p => p.Assets)
                .ThenInclude(a => a.AssetLots)
            .FirstAsync(p => p.Id == portfolio.Id);

        updated.Assets.Should().HaveCount(1);
        updated.Assets.First().TotalQuantity.Should().Be(5);
    }

    [Fact]
    public async Task Handle_Should_Fetch_Price_From_Service_When_Not_Provided()
    {
        var portfolio = await CreatePortfolioWithBalanceAndAssetAsync(10000, "AAPL", 10, 150.00m);
        _stockPricingServiceMock
            .Setup(s => s.GetCurrentPriceAsync("AAPL"))
            .ReturnsAsync(200.00m);

        var command = new SellAssetCommand(portfolio.Id, _ownerId, "AAPL", 5);

        await _handler.Handle(command, CancellationToken.None);

        _stockPricingServiceMock.Verify(s => s.GetCurrentPriceAsync("AAPL"), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Not_Call_Service_When_Price_Provided()
    {
        var portfolio = await CreatePortfolioWithBalanceAndAssetAsync(10000, "AAPL", 10, 150.00m);
        var command = new SellAssetCommand(portfolio.Id, _ownerId, "AAPL", 5, 200.00m);

        await _handler.Handle(command, CancellationToken.None);

        _stockPricingServiceMock.Verify(s => s.GetCurrentPriceAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Not_Enough_Shares()
    {
        var portfolio = await CreatePortfolioWithBalanceAndAssetAsync(10000, "AAPL", 5, 150.00m);
        var command = new SellAssetCommand(portfolio.Id, _ownerId, "AAPL", 20, 200.00m);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*don't own enough shares*");
    }

    [Fact]
    public async Task Handle_Should_Remove_Asset_When_All_Shares_Sold()
    {
        var portfolio = await CreatePortfolioWithBalanceAndAssetAsync(10000, "AAPL", 10, 150.00m);
        var command = new SellAssetCommand(portfolio.Id, _ownerId, "AAPL", 10, 200.00m);

        await _handler.Handle(command, CancellationToken.None);

        var updated = await _context.Portfolios
            .Include(p => p.Assets)
            .FirstAsync(p => p.Id == portfolio.Id);

        updated.Assets.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Portfolio_Missing()
    {
        var command = new SellAssetCommand(Guid.NewGuid(), _ownerId, "AAPL", 5, 200.00m);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Portfolio_Belongs_To_Other_User()
    {
        var portfolio = await CreatePortfolioWithBalanceAndAssetAsync(10000, "AAPL", 10, 150.00m);
        var differentUserId = Guid.NewGuid();
        var command = new SellAssetCommand(portfolio.Id, differentUserId, "AAPL", 5, 200.00m);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}