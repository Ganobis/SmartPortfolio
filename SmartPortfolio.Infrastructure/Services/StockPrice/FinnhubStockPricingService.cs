using Microsoft.Extensions.Configuration;
using SmartPortfolio.Domain.Interfaces;
using System.Net.Http.Json;

namespace SmartPortfolio.Infrastructure.Services.StockPrice;
public class FinnhubStockPricingService : IStockPricingService
{
    private readonly HttpClient _httpClient;
    private readonly string _finnhubApiKey;

    public FinnhubStockPricingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _finnhubApiKey = configuration["FinnhubSettings:ApiKey"]
                         ?? throw new ArgumentNullException("Define Finnhub ApiKey in appsettings.json!");
    }

    public async Task<decimal> GetCurrentPriceAsync(string ticker)
    {
        var relativeUrl = $"quote?symbol={ticker.ToUpper()}&token={_finnhubApiKey}";

        var response = await _httpClient.GetFromJsonAsync<FinnhubQuoteResponse>(relativeUrl);

        if (response is null || response.CurrentPrice == 0)
        {
            throw new InvalidOperationException($"Could not fetch live price for ticker '{ticker}'. Ensure the ticker is correct.");
        }

        return response.CurrentPrice;
    }
}