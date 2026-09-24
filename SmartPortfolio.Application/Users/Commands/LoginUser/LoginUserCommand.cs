using MediatR;

namespace SmartPortfolio.Application.Users.Commands.LoginUser;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<string>;