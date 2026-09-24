using MediatR;

namespace SmartPortfolio.Application.Portfolios.Commands.DepositFunds;

public record DepositFundsCommand(
    Guid PortfolioId,
    Guid UserId,
    decimal Amount,
    string Currency
) : IRequest;