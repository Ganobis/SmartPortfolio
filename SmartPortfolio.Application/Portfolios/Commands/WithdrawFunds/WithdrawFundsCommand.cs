using MediatR;

namespace SmartPortfolio.Application.Portfolios.Commands.WithdrawFunds;

public record WithdrawFundsCommand(
    Guid PortfolioId,
    Guid UserId,
    decimal Amount,
    string Currency
) : IRequest;