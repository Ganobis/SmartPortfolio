using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartPortfolio.Application.Portfolios.Queries.GetPortfolioAssets;
using SmartPortfolio.Domain.Entities;
using SmartPortfolio.Domain.ValueObjects;
using SmartPortfolio.Infrastructure.Persistence;

namespace SmartPortfolio.API.Tests.UnitTests;

public class GetPortfolioAssetsQueryHandlerTests
{
    private readonly SmartPortfolioDbContext _context;
    private readonly GetPortfolioAssetsQueryHandler _handler;
    private readonly Guid _ownerId = Guid.NewGuid();

    public GetPortfolioAssetsQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<SmartPortfolioDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new SmartPortfolioDbContext(options);

        _handler = new GetPortfolioAssetsQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_Should_Return_Assets_When_Portfolio_Exists()
    {
        var portfolio = new Portfolio("Main Portfolio", _ownerId, "USD");
        portfolio.Deposit(new Money(10000, "USD"));
        portfolio.BuyAsset("AAPL", 10, 150.00m);
        portfolio.BuyAsset("MSFT", 5, 300.00m);

        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();

        var query = new GetPortfolioAssetsQuery(portfolio.Id, _ownerId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Ticker == "AAPL" && a.TotalQuantity == 10);
        result.Should().Contain(a => a.Ticker == "MSFT" && a.TotalQuantity == 5);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_Portfolio_Has_No_Assets()
    {
        var portfolio = new Portfolio("Empty Portfolio", _ownerId, "USD");
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();

        var query = new GetPortfolioAssetsQuery(portfolio.Id, _ownerId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Throw_KeyNotFoundException_When_Portfolio_Not_Found()
    {
        var query = new GetPortfolioAssetsQuery(Guid.NewGuid(), _ownerId);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Portfolio not found or you don't have access to it.*");
    }

    [Fact]
    public async Task Handle_Should_Throw_KeyNotFoundException_When_Portfolio_Belongs_To_Different_User()
    {
        var otherOwnerId = Guid.NewGuid();
        var portfolio = new Portfolio("Other User Portfolio", otherOwnerId, "USD");
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();

        var query = new GetPortfolioAssetsQuery(portfolio.Id, _ownerId);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Portfolio not found or you don't have access to it.*");
    }
}