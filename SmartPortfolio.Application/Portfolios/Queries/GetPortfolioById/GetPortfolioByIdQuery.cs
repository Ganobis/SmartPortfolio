using MediatR;
using SmartPortfolio.API.Dtos.Portfolios;

namespace SmartPortfolio.Application.Portfolios.Queries.GetPortfolioById;

public record GetPortfolioByIdQuery(Guid PortfolioId, Guid UserId) : IRequest<PortfolioDto>;