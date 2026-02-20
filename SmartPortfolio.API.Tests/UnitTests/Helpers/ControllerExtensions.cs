using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartPortfolio.API.Tests.UnitTests.Helpers;

public static class ControllerExtensions
{
    public static T MockCurrentUser<T>(this T controller, Guid userId) where T : ControllerBase
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        controller.ControllerContext ??= new ControllerContext();
        controller.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };
        return controller;
    }
}
