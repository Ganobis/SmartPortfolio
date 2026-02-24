using SmartPortfolio.Domain.Interfaces;

namespace SmartPortfolio.Infrastructure.Services.StockPrice;
public class FakeStockPricingService : IStockPricingService
{
    private readonly Dictionary<string, decimal> _mockedPrices = new()
    {
        { "AAPL", 150.00m },
        { "MSFT", 300.00m },
        { "TSLA", 200.00m },
        { "CDPROJEKT", 115.50m }
    };

    public Task<decimal> GetCurrentPriceAsync(string ticker)
    {
        if (_mockedPrices.TryGetValue(ticker, out var price))
        {
            return Task.FromResult(price);
        }

        var deterministicRandomPrice = (decimal)(Math.Abs(ticker.GetHashCode()) % 400) + 10.50m;

        return Task.FromResult(deterministicRandomPrice);
    }
}
