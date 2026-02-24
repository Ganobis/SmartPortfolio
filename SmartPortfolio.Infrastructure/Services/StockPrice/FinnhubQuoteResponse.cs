using System.Text.Json.Serialization;

namespace SmartPortfolio.Infrastructure.Services.StockPrice;

public class FinnhubQuoteResponse
{
    [JsonPropertyName("c")]
    public decimal CurrentPrice { get; set; }
}