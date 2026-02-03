namespace SmartPortfolio.API.Dtos;
using System.Collections.Generic;

public record PortfolioDto(
    Guid Id, 
    string Name,
    decimal BalanceAmount, 
    string Currency,
    IEnumerable<TransactionDto> Transactions
    );
