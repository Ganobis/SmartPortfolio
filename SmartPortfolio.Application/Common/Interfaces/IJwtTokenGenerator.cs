using SmartPortfolio.Domain.Entities;

namespace SmartPortfolio.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}