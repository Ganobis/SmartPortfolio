using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPortfolio.Application.Common.Interfaces;
using SmartPortfolio.Domain.Interfaces;

namespace SmartPortfolio.Application.Portfolios.Commands.SellAsset;

public class SellAssetCommandHandler : IRequestHandler<SellAssetCommand>
{
    private readonly ISmartPortfolioDbContext _dbContext;
    private readonly IStockPricingService _stockPricingService;

    public SellAssetCommandHandler(ISmartPortfolioDbContext dbContext, IStockPricingService stockPricingService)
    {
        _dbContext = dbContext;
        _stockPricingService = stockPricingService;
    }

    public async Task Handle(SellAssetCommand request, CancellationToken cancellationToken)
    {
        var portfolio = await _dbContext.Portfolios
            .Include(p => p.Assets)
                .ThenInclude(a => a.AssetLots)
            .Include(p => p.Transactions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == request.PortfolioId && p.OwnerId == request.UserId, cancellationToken);

        if (portfolio is null)
        {
            throw new KeyNotFoundException("Portfolio not found or you don't have access to it.");
        }

        var priceToUse = request.SellPricePerShare ?? await _stockPricingService.GetCurrentPriceAsync(request.Ticker);

        portfolio.SellAsset(request.Ticker, request.Quantity, priceToUse);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}