using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SmartPortfolio.API.Dtos.Portfolios;
using SmartPortfolio.API.Dtos.Transactions;
using SmartPortfolio.API.Tests.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace SmartPortfolio.API.Tests.IntegrationTests;

public class PortfolioIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;

    public PortfolioIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _client.AuthenticateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

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

        var depositDto = new CreateTransactionDto(100, "PLN");
        var portfolioId = createdPortfolio.Id;

        var depositResponse = await _client.PostAsJsonAsync($"/api/portfolios/{portfolioId}/deposit", depositDto);
        depositResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/portfolios/{portfolioId}");
        getResponse.EnsureSuccessStatusCode();

        var updatedPortfolio = await getResponse.Content.ReadFromJsonAsync<PortfolioDto>();

        updatedPortfolio.Should().NotBeNull();
        updatedPortfolio!.BalanceAmount.Should().Be(100);
    }

    [Fact]
    public async Task Should_Return_Correct_Portfolio_Value_In_PLN()
    {
        var createDto = new { Name = "Test Wallet USD", Currency = "USD" };
        var createResponse = await _client.PostAsJsonAsync("/api/portfolios", createDto);
        createResponse.EnsureSuccessStatusCode();

        var portfolio = await createResponse.Content.ReadFromJsonAsync<PortfolioDto>();
        var portfolioId = portfolio!.Id;

        var depositDto = new { Amount = 100m, Currency = "USD" };
        await _client.PostAsJsonAsync($"/api/portfolios/{portfolioId}/deposit", depositDto);

        var response = await _client.GetAsync($"/api/portfolios/{portfolioId}/value?currency=PLN");

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        json.GetProperty("convertedAmount").GetDecimal().Should().Be(400.00m);

        json.GetProperty("targetCurrency").GetString().Should().Be("PLN");
    }

    [Fact]
    public async Task Should_Calculate_Value_In_EUR_Using_Cross_Rates()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/portfolios", new CreatePortfolioDto("Europe Trip", "USD"));
        var portfolio = await createResponse.Content.ReadFromJsonAsync<PortfolioDto>();

        await _client.PostAsJsonAsync($"/api/portfolios/{portfolio!.Id}/deposit", new CreateTransactionDto(100, "USD"));

        var response = await _client.GetAsync($"/api/portfolios/{portfolio.Id}/value?currency=EUR");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        json.GetProperty("convertedAmount").GetDecimal().Should().Be(93.02m);
        json.GetProperty("targetCurrency").GetString().Should().Be("EUR");
    }

    [Fact]
    public async Task Should_Return_NotFound_For_NonExistent_Portfolio()
    {
        var randomId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/portfolios/{randomId}/value?currency=PLN");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Return_Transaction_History_For_Portfolio()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/portfolios", new CreatePortfolioDto("Savings", "USD"));
        createResponse.EnsureSuccessStatusCode();
        var portfolio = await createResponse.Content.ReadFromJsonAsync<PortfolioDto>();
        var portfolioId = portfolio!.Id;

        await _client.PostAsJsonAsync($"/api/portfolios/{portfolioId}/deposit", new CreateTransactionDto(100, "USD"));
        await _client.PostAsJsonAsync($"/api/portfolios/{portfolioId}/deposit", new CreateTransactionDto(50, "USD"));

        var historyResponse = await _client.GetAsync($"/api/portfolios/{portfolioId}/transactions");

        historyResponse.EnsureSuccessStatusCode();
        var transactions = await historyResponse.Content.ReadFromJsonAsync<List<TransactionDto>>();

        transactions.Should().NotBeNull();
        transactions!.Count.Should().Be(2); 
        
        transactions[0].Amount.Should().Be(50);
        transactions[1].Amount.Should().Be(100);
    }
}
