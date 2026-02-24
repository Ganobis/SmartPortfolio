namespace SmartPortfolio.API.Dtos.Portfolios;
public record PortfolioValueDto
(
    Guid PortfolioId,
    decimal OriginalAmount,
    string OriginalCurrency,
    decimal ConvertedAmount,
    string TargetCurrency
);