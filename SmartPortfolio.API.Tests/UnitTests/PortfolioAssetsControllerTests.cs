using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartPortfolio.API.Controllers;
using SmartPortfolio.API.Dtos.Assets;
using SmartPortfolio.API.Tests.UnitTests.Helpers;
using SmartPortfolio.Application.Portfolios.Commands.BuyAsset;
using SmartPortfolio.Application.Portfolios.Commands.SellAsset;
using SmartPortfolio.Application.Portfolios.Queries.GetPortfolioAssets;

namespace SmartPortfolio.API.Tests.UnitTests;

public class PortfolioAssetsControllerTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly PortfolioAssetsController _controller;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _portfolioId = Guid.NewGuid();

    public PortfolioAssetsControllerTests()
    {
        _senderMock = new Mock<ISender>();
        _controller = new PortfolioAssetsController(_senderMock.Object);
        _controller.MockCurrentUser(_userId);
    }

    [Fact]
    public async Task GetAll_Should_Send_GetPortfolioAssetsQuery_And_Return_Ok()
    {
        var expectedAssets = new List<AssetDto>
        {
            new(Guid.NewGuid(), "AAPL", 10),
            new(Guid.NewGuid(), "MSFT", 5)
        };

        _senderMock
            .Setup(s => s.Send(It.Is<GetPortfolioAssetsQuery>(q => q.PortfolioId == _portfolioId && q.UserId == _userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAssets);

        var result = await _controller.GetAll(_portfolioId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var assets = okResult.Value.Should().BeAssignableTo<List<AssetDto>>().Subject;
        assets.Should().HaveCount(2);
        _senderMock.Verify(s => s.Send(It.IsAny<GetPortfolioAssetsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Buy_Should_Send_BuyAssetCommand_And_Return_NoContent()
    {
        var dto = new BuyAssetDto("AAPL", 5, 150.00m);

        var result = await _controller.Buy(_portfolioId, dto);

        result.Should().BeOfType<NoContentResult>();
        _senderMock.Verify(s => s.Send(
            It.Is<BuyAssetCommand>(c =>
                c.PortfolioId == _portfolioId &&
                c.UserId == _userId &&
                c.Ticker == "AAPL" &&
                c.Quantity == 5 &&
                c.PricePerShare == 150.00m),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Sell_Should_Send_SellAssetCommand_And_Return_NoContent()
    {
        var dto = new SellAssetDto("AAPL", 5, 200.00m);

        var result = await _controller.Sell(_portfolioId, dto);

        result.Should().BeOfType<NoContentResult>();
        _senderMock.Verify(s => s.Send(
            It.Is<SellAssetCommand>(c =>
                c.PortfolioId == _portfolioId &&
                c.UserId == _userId &&
                c.Ticker == "AAPL" &&
                c.Quantity == 5 &&
                c.SellPricePerShare == 200.00m),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}