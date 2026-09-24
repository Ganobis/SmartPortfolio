using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPortfolio.Application.Common.Interfaces;
using SmartPortfolio.Domain.Interfaces;

namespace SmartPortfolio.Application.Portfolios.Commands.BuyAsset;

public class BuyAssetCommandHandler : IRequestHandler<BuyAssetCommand>
{
    private readonly ISmartPortfolioDbContext _dbContext;
    private readonly IStockPricingService _stockPricingService;

    public BuyAssetCommandHandler(ISmartPortfolioDbContext dbContext, IStockPricingService stockPricingService)
    {
        _dbContext = dbContext;
        _stockPricingService = stockPricingService;
    }

    public async Task Handle(BuyAssetCommand request, CancellationToken cancellationToken)
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

        var priceToUse = request.PricePerShare ?? await _stockPricingService.GetCurrentPriceAsync(request.Ticker);

        portfolio.BuyAsset(request.Ticker, request.Quantity, priceToUse, request.PurchaseDate);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}