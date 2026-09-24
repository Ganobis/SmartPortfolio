using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPortfolio.API.Dtos.Portfolios;
using SmartPortfolio.API.Dtos.Transactions;
using SmartPortfolio.Application.Common.Interfaces;

namespace SmartPortfolio.Application.Portfolios.Queries.GetPortfolioById;

public class GetPortfolioByIdQueryHandler : IRequestHandler<GetPortfolioByIdQuery, PortfolioDto>
{
    private readonly ISmartPortfolioDbContext _dbContext;

    public GetPortfolioByIdQueryHandler(ISmartPortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PortfolioDto> Handle(GetPortfolioByIdQuery request, CancellationToken cancellationToken)
    {
        var portfolio = await _dbContext.Portfolios
            .Include(p => p.Transactions)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PortfolioId && p.OwnerId == request.UserId, cancellationToken);

        if (portfolio is null)
        {
            throw new KeyNotFoundException("Portfolio not found or you don't have access to it.");
        }

        var transactionsDto = portfolio.Transactions
            .Select(t => new TransactionDto(t.Id, t.Amount, t.Currency, t.Timestamp))
            .ToList();

        return new PortfolioDto(
            portfolio.Id,
            portfolio.Name,
            portfolio.Balance.Amount,
            portfolio.Balance.Currency,
            transactionsDto
        );
    }
}