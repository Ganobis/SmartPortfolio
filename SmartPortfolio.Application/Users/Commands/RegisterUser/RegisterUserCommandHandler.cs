using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartPortfolio.Application.Common.Interfaces;
using SmartPortfolio.Domain.Entities;

namespace SmartPortfolio.Application.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly ISmartPortfolioDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public RegisterUserCommandHandler(
        ISmartPortfolioDbContext dbContext,
        IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (userExists)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var dummyUser = new User(request.Email, string.Empty, request.Name);
        var hashedPassword = _passwordHasher.HashPassword(dummyUser, request.Password);

        var user = new User(request.Email, hashedPassword, request.Name);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}