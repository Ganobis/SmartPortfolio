using MediatR;
using SmartPortfolio.API.Dtos.Portfolios;

namespace SmartPortfolio.Application.Portfolios.Commands.CreatePortfolio;

public record CreatePortfolioCommand(
    Guid UserId,
    string Name,
    string Currency
) : IRequest<PortfolioDto>;