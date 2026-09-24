using MediatR;
using SmartPortfolio.API.Dtos.Assets;

namespace SmartPortfolio.Application.Portfolios.Queries.GetPortfolioAssets;

public record GetPortfolioAssetsQuery(Guid PortfolioId, Guid UserId) : IRequest<List<AssetDto>>;