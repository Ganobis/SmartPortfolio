using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartPortfolio.API.Dtos.Assets;
using SmartPortfolio.Application.Common.Interfaces;

namespace SmartPortfolio.Application.Portfolios.Queries.GetPortfolioAssets;

public class GetPortfolioAssetsQueryHandler : IRequestHandler<GetPortfolioAssetsQuery, List<AssetDto>>
{
    private readonly ISmartPortfolioDbContext _dbContext;

    public GetPortfolioAssetsQueryHandler(ISmartPortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AssetDto>> Handle(GetPortfolioAssetsQuery request, CancellationToken cancellationToken)
    {
        var portfolio = await _dbContext.Portfolios
            .Include(p => p.Assets)
                .ThenInclude(a => a.AssetLots)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PortfolioId && p.OwnerId == request.UserId, cancellationToken);

        if (portfolio is null)
        {
            throw new KeyNotFoundException("Portfolio not found or you don't have access to it.");
        }

        return portfolio.Assets
            .Select(a => new AssetDto(a.Id, a.Ticker, a.TotalQuantity))
            .ToList();
    }
}