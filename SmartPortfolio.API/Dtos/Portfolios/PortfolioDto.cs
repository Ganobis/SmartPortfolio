namespace SmartPortfolio.API.Dtos.Portfolios;

using SmartPortfolio.API.Dtos.Transactions;
using System.Collections.Generic;

public record PortfolioDto(
    Guid Id, 
    string Name,
    decimal BalanceAmount, 
    string Currency,
    IEnumerable<TransactionDto> Transactions
    );
