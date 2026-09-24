using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartPortfolio.Application.Common.Interfaces;
using SmartPortfolio.Domain.Entities;

namespace SmartPortfolio.Application.Users.Commands.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, string>
{
    private readonly ISmartPortfolioDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserCommandHandler(
        ISmartPortfolioDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<string> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user is null)
        {
            throw new UnauthorizedAccessException("Wrong email or password.");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Wrong email or password.");
        }

        return _jwtTokenGenerator.GenerateToken(user);
    }
}