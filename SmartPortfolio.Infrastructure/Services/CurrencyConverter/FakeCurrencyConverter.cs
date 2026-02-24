using SmartPortfolio.Domain.Interfaces;

namespace SmartPortfolio.Infrastructure.Services.CurrencyConverter;

public class FakeCurrencyConverter : ICurrencyConverter
{
    private readonly Dictionary<string, decimal> _ratesToPln = new()
    {
        { "PLN", 1.0m },
        { "USD", 4.0m },
        { "EUR", 4.3m },
        { "GBP", 5.0m },
        { "CHF", 4.5m }
    };
    public Task<decimal> Convert(decimal amount, string fromCurrency, string toCurrency)
    {
        var from = fromCurrency.ToUpper();
        var to = toCurrency.ToUpper();

        if (!_ratesToPln.ContainsKey(from) || !_ratesToPln.ContainsKey(to))
        {
            throw new ArgumentException($"Nieobsługiwana para walutowa: {from} -> {to}");
        }
        var rateFrom = _ratesToPln[from];
        var rateTo = _ratesToPln[to];

        var result = amount * (rateFrom / rateTo);

        return Task.FromResult(Math.Round(result, 2));
    }
}
