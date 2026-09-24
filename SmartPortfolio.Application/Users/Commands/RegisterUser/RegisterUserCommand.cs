using MediatR;

namespace SmartPortfolio.Application.Users.Commands.RegisterUser;

public record RegisterUserCommand(
    string Email,
    string Name,
    string Password
) : IRequest<Guid>;