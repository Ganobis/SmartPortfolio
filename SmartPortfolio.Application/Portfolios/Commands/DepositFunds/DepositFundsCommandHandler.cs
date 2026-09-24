using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPortfolio.Application.Common.Interfaces;
using SmartPortfolio.Domain.ValueObjects;

namespace SmartPortfolio.Application.Portfolios.Commands.DepositFunds;

public class DepositFundsCommandHandler : IRequestHandler<DepositFundsCommand>
{
    private readonly ISmartPortfolioDbContext _dbContext;

    public DepositFundsCommandHandler(ISmartPortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DepositFundsCommand request, CancellationToken cancellationToken)
    {
        var portfolio = await _dbContext.Portfolios
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.Id == request.PortfolioId && p.OwnerId == request.UserId, cancellationToken);

        if (portfolio is null)
        {
            throw new KeyNotFoundException("Portfolio not found or you don't have access to it.");
        }

        var money = new Money(request.Amount, request.Currency);
        portfolio.Deposit(money);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}