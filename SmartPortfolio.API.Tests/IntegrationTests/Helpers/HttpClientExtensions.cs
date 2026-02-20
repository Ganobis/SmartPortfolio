using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartPortfolio.API.Tests.IntegrationTests.Helpers;

public static class HttpClientExtensions
{
    public static async Task AuthenticateAsync(this HttpClient client, string? userId = null)
    {
        var email = $"testuser_{Guid.NewGuid()}@test.com";
        var password = "TestPassword123!";

        var registerDto = new { Name = "Test User", Email = email, Password = password };
        var registerResponse = await client.PostAsJsonAsync("/api/users/register", registerDto);
        registerResponse.EnsureSuccessStatusCode();

        var loginDto = new { Email = email, Password = password };
        var loginResponse = await client.PostAsJsonAsync("/api/users/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();

        var result = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = result.GetProperty("token").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}