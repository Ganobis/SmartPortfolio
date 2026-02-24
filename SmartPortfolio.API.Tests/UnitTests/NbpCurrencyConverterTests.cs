using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using SmartPortfolio.Infrastructure.Services.CurrencyConverter;
using System.Net;
using System.Text.Json;

namespace SmartPortfolio.API.Tests.UnitTests;
public class NbpCurrencyConverterTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly NbpCurrencyConverter _converter;

    public NbpCurrencyConverterTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://fake-nbp-api.com/")
        };

        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _converter = new NbpCurrencyConverter(_httpClient, _memoryCache);
    }

    [Fact]
    public async Task ConvertAsync_Should_Calculate_Rate_Correctly_From_Api()
    {
        var fakeNbpResponse = new List<object>
            {
                new {
                    table = "A",
                    no = "123/A/NBP/2026",
                    effectiveDate = "2026-02-08",
                    rates = new[] {
                        new { currency = "dolar amerykański", code = "USD", mid = 4.00m }
                    }
                }
            };
        var jsonResponse = JsonSerializer.Serialize(fakeNbpResponse);

        SetupMockHttpResponse(HttpStatusCode.OK, jsonResponse);

        var result = await _converter.Convert(100m, "USD", "PLN");

        result.Should().Be(400.00m);
    }

    [Fact]
    public async Task ConvertAsync_Should_Use_Cache_And_Call_Api_Only_Once()
    {
        var jsonResponse = JsonSerializer.Serialize(new List<object> {
                new {
                    table = "A", no = "xyz", effectiveDate = "2022-01-01",
                    rates = new[] { new { currency = "dolar", code = "USD", mid = 4.00m } } }
            });

        SetupMockHttpResponse(HttpStatusCode.OK, jsonResponse);

        await _converter.Convert(10m, "USD", "PLN");
        await _converter.Convert(50m, "USD", "PLN");

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task ConvertAsync_Should_Throw_When_Currency_Not_Supported()
    {
        var jsonResponse = JsonSerializer.Serialize(new List<object> {
                new {
                    table = "A", no = "xyz", effectiveDate = "2022-01-01",
                    rates = new[] { new { currency = "dolar", code = "USD", mid = 4.00m } } }
            });
        SetupMockHttpResponse(HttpStatusCode.OK, jsonResponse);

        Func<Task> act = async () => await _converter.Convert(100, "JPY", "PLN");

        await act.Should().ThrowAsync<System.ArgumentException>();
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