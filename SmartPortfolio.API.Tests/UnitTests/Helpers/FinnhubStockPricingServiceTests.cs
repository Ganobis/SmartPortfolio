using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using SmartPortfolio.Infrastructure.Services.StockPrice;
using System.Net;
using System.Text.Json;

namespace SmartPortfolio.API.Tests.UnitTests;
public class FinnhubStockPricingServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly FinnhubStockPricingService _service;

    public FinnhubStockPricingServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://fake-finnhub-api.com/")
        };

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["FinnhubSettings:ApiKey"]).Returns("fake-api-key");

        _service = new FinnhubStockPricingService(_httpClient, configMock.Object);
    }

    [Fact]
    public async Task GetCurrentPriceAsync_Should_Return_Price_From_Api()
    {
        var fakeResponse = new { c = 150.25m };
        var jsonResponse = JsonSerializer.Serialize(fakeResponse);

        SetupMockHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _service.GetCurrentPriceAsync("AAPL");

        result.Should().Be(150.25m);
    }

    [Fact]
    public async Task GetCurrentPriceAsync_Should_Send_Uppercase_Ticker_In_Request()
    {
        var fakeResponse = new { c = 300.00m };
        var jsonResponse = JsonSerializer.Serialize(fakeResponse);

        SetupMockHttpResponse(HttpStatusCode.OK, jsonResponse);

        await _service.GetCurrentPriceAsync("msft");

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri!.ToString().Contains("symbol=MSFT")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetCurrentPriceAsync_Should_Throw_When_Response_Is_Null()
    {
        SetupMockHttpResponse(HttpStatusCode.OK, "null");

        Func<Task> act = async () => await _service.GetCurrentPriceAsync("INVALID");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Could not fetch live price*");
    }

    [Fact]
    public async Task GetCurrentPriceAsync_Should_Throw_When_CurrentPrice_Is_Zero()
    {
        var fakeResponse = new { c = 0m };
        var jsonResponse = JsonSerializer.Serialize(fakeResponse);

        SetupMockHttpResponse(HttpStatusCode.OK, jsonResponse);

        Func<Task> act = async () => await _service.GetCurrentPriceAsync("AAPL");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Could not fetch live price*");
    }

    [Fact]
    public void Constructor_Should_Throw_When_ApiKey_Is_Missing()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["FinnhubSettings:ApiKey"]).Returns((string?)null);

        Action act = () => new FinnhubStockPricingService(_httpClient, configMock.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetCurrentPriceAsync_Should_Include_ApiKey_In_Request()
    {
        var fakeResponse = new { c = 200.00m };
        var jsonResponse = JsonSerializer.Serialize(fakeResponse);

        SetupMockHttpResponse(HttpStatusCode.OK, jsonResponse);

        await _service.GetCurrentPriceAsync("TSLA");

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri!.ToString().Contains("token=fake-api-key")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    private void SetupMockHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }
}