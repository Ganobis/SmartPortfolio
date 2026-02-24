namespace SmartPortfolio.API.Dtos.Assets;
public record SellAssetDto(
    string Ticker,
    decimal Quantity, 
    decimal? SellPricePerShare = null
);