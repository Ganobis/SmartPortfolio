using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPortfolio.API.Dtos.Portfolios;
using SmartPortfolio.Application.Common.Interfaces;
using SmartPortfolio.Domain.Interfaces;

namespace SmartPortfolio.Application.Portfolios.Queries.GetPortfolioValue;

public class GetPortfolioValueQueryHandler : IRequestHandler<GetPortfolioValueQuery, PortfolioValueDto>
{
    private readonly ISmartPortfolioDbContext _dbContext;
    private readonly ICurrencyConverter _currencyConverter;

    public GetPortfolioValueQueryHandler(ISmartPortfolioDbContext dbContext, ICurrencyConverter currencyConverter)
    {
        _dbContext = dbContext;
        _currencyConverter = currencyConverter;
    }

    public async Task<PortfolioValueDto> Handle(GetPortfolioValueQuery request, CancellationToken cancellationToken)
    {
        var portfolio = await _dbContext.Portfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PortfolioId && p.OwnerId == request.UserId, cancellationToken);

        if (portfolio is null)
        {
            throw new KeyNotFoundException("Portfolio not found or you don't have access to it.");
        }

        var convertedAmount = await _currencyConverter.Convert(
            portfolio.Balance.Amount,
            portfolio.Balance.Currency,
            request.TargetCurrency
        );

        return new PortfolioValueDto(
            portfolio.Id,
            portfolio.Balance.Amount,
            portfolio.Balance.Currency,
            convertedAmount,
            request.TargetCurrency.ToUpper()
        );
    }
}
