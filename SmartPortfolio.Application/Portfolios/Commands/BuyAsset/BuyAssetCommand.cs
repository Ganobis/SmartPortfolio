using MediatR;

namespace SmartPortfolio.Application.Portfolios.Commands.BuyAsset;

public record BuyAssetCommand(
    Guid PortfolioId,
    Guid UserId,
    string Ticker,
    decimal Quantity,
    decimal? PricePerShare = null,
    DateTime? PurchaseDate = null
) : IRequest;