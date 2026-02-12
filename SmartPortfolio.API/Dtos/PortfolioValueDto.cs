namespace SmartPortfolio.API.Dtos;
public record PortfolioValueDto
(
    Guid PortfolioId,
    decimal OriginalAmount,
    string OriginalCurrency,
    decimal ConvertedAmount,
    string TargetCurrency
);