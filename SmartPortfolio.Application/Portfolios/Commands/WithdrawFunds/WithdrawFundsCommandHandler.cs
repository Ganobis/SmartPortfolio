using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPortfolio.Application.Common.Interfaces;
using SmartPortfolio.Domain.ValueObjects;

namespace SmartPortfolio.Application.Portfolios.Commands.WithdrawFunds;

public class WithdrawFundsCommandHandler : IRequestHandler<WithdrawFundsCommand>
{
    private readonly ISmartPortfolioDbContext _dbContext;

    public WithdrawFundsCommandHandler(ISmartPortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(WithdrawFundsCommand request, CancellationToken cancellationToken)
    {
        var portfolio = await _dbContext.Portfolios
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.Id == request.PortfolioId && p.OwnerId == request.UserId, cancellationToken);

        if (portfolio is null)
        {
            throw new KeyNotFoundException("Portfolio not found or you don't have access to it.");
        }

        var money = new Money(request.Amount, request.Currency);
        portfolio.Withdraw(money);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}