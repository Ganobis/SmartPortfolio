using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPortfolio.API.Dtos.Transactions;
using SmartPortfolio.Application.Common.Interfaces;

namespace SmartPortfolio.Application.Portfolios.Queries.GetPortfolioTransactions;

public class GetPortfolioTransactionsQueryHandler : IRequestHandler<GetPortfolioTransactionsQuery, List<TransactionDto>>
{
    private readonly ISmartPortfolioDbContext _dbContext;

    public GetPortfolioTransactionsQueryHandler(ISmartPortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TransactionDto>> Handle(GetPortfolioTransactionsQuery request, CancellationToken cancellationToken)
    {
        var portfolio = await _dbContext.Portfolios
            .Include(p => p.Transactions)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PortfolioId && p.OwnerId == request.UserId, cancellationToken);

        if (portfolio is null)
        {
            throw new KeyNotFoundException("Portfolio not found or you don't have access to it.");
        }

        return portfolio.Transactions
            .OrderByDescending(t => t.Timestamp)
            .Select(t => new TransactionDto(t.Id, t.Amount, t.Currency, t.Timestamp))
            .ToList();
    }
}