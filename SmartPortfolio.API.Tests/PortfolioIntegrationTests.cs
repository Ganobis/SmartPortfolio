using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SmartPortfolio.API.Dtos;
using SmartPortfolio.API;
using Xunit;

namespace SmartPortfolio.API.Tests;

public class PortfolioIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PortfolioIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Create_Portfolio_And_Deposit_Funds()
    {
        var createDto = new CreatePortfolioDto("Test Integration", "PLN");
        var createResponse = await _client.PostAsJsonAsync("/api/portfolios", createDto);
        createResponse.EnsureSuccessStatusCode();

        var createdPortfolio = await createResponse.Content.ReadFromJsonAsync<PortfolioDto>();
        createdPortfolio.Should().NotBeNull();
        createdPortfolio!.Id.Should().NotBeEmpty();
        createdPortfolio.BalanceAmount.Should().Be(0);

        var depositDto = new TransactionDto(100, "PLN");
        var portfolioId = createdPortfolio.Id;

        var depositResponse = await _client.PostAsJsonAsync($"/api/portfolios/{portfolioId}/deposit", depositDto);
        depositResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/portfolios/{portfolioId}");
        getResponse.EnsureSuccessStatusCode();

        var updatedPortfolio = await getResponse.Content.ReadFromJsonAsync<PortfolioDto>();

        updatedPortfolio.Should().NotBeNull();
        updatedPortfolio!.BalanceAmount.Should().Be(100);
    }
}
