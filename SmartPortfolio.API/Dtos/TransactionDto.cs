namespace SmartPortfolio.API.Dtos;

public record TransactionDto(
    Guid Id,
    decimal Amount,
    string Currency,
    DateTime Timestamp
);