namespace SmartPortfolio.Domain.Interfaces;

public interface ICurrencyConverter
{
    Task<decimal> Convert(decimal amount, string fromCurrency, string toCurrency);
}