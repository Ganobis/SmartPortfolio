using MediatR;
using SmartPortfolio.API.Dtos.Portfolios;
using SmartPortfolio.API.Dtos.Transactions;
using SmartPortfolio.Application.Common.Interfaces;
using SmartPortfolio.Domain.Entities;

namespace SmartPortfolio.Application.Portfolios.Commands.CreatePortfolio;

public class CreatePortfolioCommandHandler : IRequestHandler<CreatePortfolioCommand, PortfolioDto>
{
    private readonly ISmartPortfolioDbContext _dbContext;

    public CreatePortfolioCommandHandler(ISmartPortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PortfolioDto> Handle(CreatePortfolioCommand request, CancellationToken cancellationToken)
    {
        var portfolio = new Portfolio(request.Name, request.UserId, request.Currency);

        _dbContext.Portfolios.Add(portfolio);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PortfolioDto(
            portfolio.Id,
            portfolio.Name,
            portfolio.Balance.Amount,
            portfolio.Balance.Currency,
            new List<TransactionDto>()
        );
    }
}