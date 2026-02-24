using Microsoft.AspNetCore.Mvc;
using SmartPortfolio.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartPortfolio.API.Extensions;
public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                           user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (Guid.TryParse(userIdString, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("The user could not be identified.");
    }
}