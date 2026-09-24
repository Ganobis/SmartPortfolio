using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartPortfolio.API.Controllers;
using SmartPortfolio.API.Dtos.Portfolios;
using SmartPortfolio.API.Tests.UnitTests.Helpers;
using SmartPortfolio.Application.Portfolios.Queries.GetPortfolioValue;
using Xunit;

namespace SmartPortfolio.API.Tests.UnitTests;

public class PortfoliosControllerTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly PortfoliosController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public PortfoliosControllerTests()
    {
        _senderMock = new Mock<ISender>();
        _controller = new PortfoliosController(_senderMock.Object);
        _controller.MockCurrentUser(_userId);
    }

    [Fact]
    public async Task GetValue_Should_Return_Ok_With_Converted_Amount()
    {
        var portfolioId = Guid.NewGuid();
        var expectedDto = new PortfolioValueDto(portfolioId, 100, "USD", 90.00m, "EUR");

        _senderMock
            .Setup(s => s.Send(It.Is<GetPortfolioValueQuery>(q => q.PortfolioId == portfolioId && q.UserId == _userId && q.TargetCurrency == "EUR"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _controller.GetValue(portfolioId, "EUR");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<PortfolioValueDto>().Subject;

        value.ConvertedAmount.Should().Be(90.00m);
        value.TargetCurrency.Should().Be("EUR");
    }
}