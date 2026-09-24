using MediatR;
using SmartPortfolio.API.Dtos.Transactions;

namespace SmartPortfolio.Application.Portfolios.Queries.GetPortfolioTransactions;

public record GetPortfolioTransactionsQuery(Guid PortfolioId, Guid UserId) : IRequest<List<TransactionDto>>;