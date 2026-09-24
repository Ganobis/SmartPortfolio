using MediatR;
using SmartPortfolio.API.Dtos.Portfolios;

namespace SmartPortfolio.Application.Portfolios.Queries.GetPortfolioValue;

public record GetPortfolioValueQuery(
    Guid PortfolioId,
    Guid UserId,
    string TargetCurrency
) : IRequest<PortfolioValueDto>;