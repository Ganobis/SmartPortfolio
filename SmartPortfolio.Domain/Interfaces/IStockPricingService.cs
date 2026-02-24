namespace SmartPortfolio.Domain.Interfaces;
public interface IStockPricingService
{
    Task<decimal> GetCurrentPriceAsync(string ticker);
}
