namespace SmartPortfolio.API.Dtos.Assets;
public record BuyAssetDto(
    string Ticker,
    decimal Quantity,
    decimal? PricePerShare = null,
    DateTime? PurchaseDate = null
);