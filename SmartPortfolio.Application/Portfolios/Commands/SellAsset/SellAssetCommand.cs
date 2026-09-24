using MediatR;

namespace SmartPortfolio.Application.Portfolios.Commands.SellAsset;

public record SellAssetCommand(
    Guid PortfolioId,
    Guid UserId,
    string Ticker,
    decimal Quantity,
    decimal? SellPricePerShare = null
) : IRequest;