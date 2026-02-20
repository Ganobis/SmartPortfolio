using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Org.BouncyCastle.Ocsp;
using SmartPortfolio.API.Controllers;
using SmartPortfolio.API.Tests.UnitTests.Helpers;
using SmartPortfolio.Domain.Entities;
using SmartPortfolio.Domain.Interfaces;
using SmartPortfolio.Domain.ValueObjects;
using SmartPortfolio.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace SmartPortfolio.API.Tests.UnitTests;
public class PortfoliosControllerTests
{
    private readonly SmartPortfolioDbContext _context;
    private readonly Mock<ICurrencyConverter> _converterMock;
    private readonly PortfoliosController _controller;

    public PortfoliosControllerTests()
    {
        var options = new DbContextOptionsBuilder<SmartPortfolioDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new SmartPortfolioDbContext(options);

        _converterMock = new Mock<ICurrencyConverter>();

        _controller = new PortfoliosController(_context, _converterMock.Object);
    }



    [Fact]
    public async Task GetValue_Should_Return_Ok_With_Converted_Amount()
    {

        var ownerId = Guid.NewGuid();
        var money = new Money(100, "USD");
        var portfolio = new Portfolio("Test", ownerId, "USD");


        typeof(Portfolio).GetProperty("Balance")!.SetValue(portfolio, money);

        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();


        _converterMock.Setup(x => x.Convert(100, "USD", "EUR"))
                      .ReturnsAsync(90.00m);


        _controller.MockCurrentUser(ownerId);

        var realportfolioId = portfolio.Id;
        var result = await _controller.GetValue(realportfolioId, "EUR");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;

        dynamic value = okResult.Value!;

        ((decimal)value.ConvertedAmount).Should().Be(90.00m);
        ((string)value.TargetCurrency).Should().Be("EUR");
    }

    [Fact]
    public async Task GetValue_Should_Return_NotFound_When_Portfolio_Missing()
    {
        _controller.MockCurrentUser(new Guid());
        var result = await _controller.GetValue(Guid.NewGuid(), "PLN");
        result.Result.Should().BeOfType<NotFoundResult>();
    }
}

