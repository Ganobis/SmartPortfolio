using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartPortfolio.Application.Portfolios.Commands.BuyAsset;
using SmartPortfolio.Domain.Entities;
using SmartPortfolio.Domain.Interfaces;
using SmartPortfolio.Domain.ValueObjects;
using SmartPortfolio.Infrastructure.Persistence;

namespace SmartPortfolio.API.Tests.UnitTests;

public class BuyAssetCommandHandlerTests
{
    private readonly SmartPortfolioDbContext _context;
    private readonly Mock<IStockPricingService> _stockPricingServiceMock;
    private readonly BuyAssetCommandHandler _handler;
    private readonly Guid _ownerId = Guid.NewGuid();

    public BuyAssetCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<SmartPortfolioDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new SmartPortfolioDbContext(options);

        _stockPricingServiceMock = new Mock<IStockPricingService>();

        _handler = new BuyAssetCommandHandler(_context, _stockPricingServiceMock.Object);
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

    [Fact]
    public async Task Handle_Should_Buy_Asset_With_Explicit_Price()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(10000);
        var command = new BuyAssetCommand(portfolio.Id, _ownerId, "AAPL", 5, 150.00m);

        await _handler.Handle(command, CancellationToken.None);

        var updated = await _context.Portfolios
            .Include(p => p.Assets).ThenInclude(a => a.AssetLots)
            .FirstAsync(p => p.Id == portfolio.Id);

        updated.Assets.Should().HaveCount(1);
        updated.Assets.First().Ticker.Should().Be("AAPL");
        updated.Assets.First().TotalQuantity.Should().Be(5);
    }

    [Fact]
    public async Task Handle_Should_Fetch_Price_From_Service_When_Not_Provided()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(10000);
        _stockPricingServiceMock
            .Setup(s => s.GetCurrentPriceAsync("AAPL"))
            .ReturnsAsync(175.00m);

        var command = new BuyAssetCommand(portfolio.Id, _ownerId, "AAPL", 5);

        await _handler.Handle(command, CancellationToken.None);

        _stockPricingServiceMock.Verify(s => s.GetCurrentPriceAsync("AAPL"), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Insufficient_Funds()
    {
        var portfolio = await CreatePortfolioWithBalanceAsync(100);
        var command = new BuyAssetCommand(portfolio.Id, _ownerId, "AAPL", 10, 150.00m);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Insufficient funds*");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Portfolio_Missing()
    {
        var command = new BuyAssetCommand(Guid.NewGuid(), _ownerId, "AAPL", 5, 150.00m);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}