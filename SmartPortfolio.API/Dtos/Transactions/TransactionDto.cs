namespace SmartPortfolio.API.Dtos.Transactions;

public record TransactionDto(
    Guid Id,
    decimal Amount,
    string Currency,
    DateTime Timestamp
);