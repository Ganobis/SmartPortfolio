using Microsoft.Extensions.Caching.Memory;
using SmartPortfolio.Domain.Interfaces;
using System.Net.Http.Json;
using static SmartPortfolio.Infrastructure.Services.CurrencyConverter.NbpResponse;

namespace SmartPortfolio.Infrastructure.Services.CurrencyConverter;

public class NbpCurrencyConverter : ICurrencyConverter
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "NbpRatesTableA";

    public NbpCurrencyConverter(HttpClient httpClient, IMemoryCache memoryCache)
    {
        _httpClient = httpClient;
        _cache = memoryCache;
    }

    public async Task<decimal> Convert(decimal amount, string fromCurrency, string toCurrency)
    {
        var table = await GetRatesFromNbp();

        var rates = table.Rates.ToDictionary(k => k.Code, v => v.Mid);
        if (!rates.ContainsKey("PLN"))
        {
            rates.Add("PLN", 1.0m);
        }

        var fromCode = fromCurrency.ToUpper();
        var toCode = toCurrency.ToUpper();

        if (!rates.ContainsKey(fromCode) || !rates.ContainsKey(toCode))
        {
            throw new ArgumentException($"NBP don't support: {fromCode} or {toCode}");
        }

        var rateFrom = rates[fromCode];
        var rateTo = rates[toCode];

        return Math.Round(amount * (rateFrom / rateTo), 2);
    }

    private async Task<NbpTable> GetRatesFromNbp()
    {
        if (!_cache.TryGetValue(CacheKey, out NbpTable? table))
        {
            var response = await _httpClient.GetFromJsonAsync<List<NbpTable>>("");

            if (response == null || response.Count == 0)
            {
                throw new InvalidOperationException("No rates returned from NBP API.");
            }

            table = response[0];

            var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(CacheKey, table, cacheOptions);
        }

        return table!;
    }
}
