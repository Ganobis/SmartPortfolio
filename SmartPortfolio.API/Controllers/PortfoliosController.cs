using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartPortfolio.API.Dtos;
using SmartPortfolio.Domain.Entities;
using SmartPortfolio.Domain.Interfaces;
using SmartPortfolio.Domain.ValueObjects;
using SmartPortfolio.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartPortfolio.API.Controllers;

[Route("api/portfolios")]
[ApiController]
[Authorize]
public class PortfoliosController : ControllerBase
{
    private readonly SmartPortfolioDbContext _dbContext;
    private readonly ICurrencyConverter _currencyConverter;

    public PortfoliosController(SmartPortfolioDbContext dbContext, ICurrencyConverter currencyConverter)
    {
        _dbContext = dbContext;
        _currencyConverter = currencyConverter;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PortfolioDto>> Get(Guid id)
    {
        var portfolio = await GetUserPortfolioAsync(id);
        if (portfolio is null) return NotFound();

        var transactionsDto = portfolio.Transactions
            .Select(t => new TransactionDto(t.Id, t.Amount, t.Currency, t.Timestamp))
            .ToList();

        var dto = new PortfolioDto
        (
            portfolio.Id,
            portfolio.Name,
            portfolio.Balance.Amount,
            portfolio.Balance.Currency,
            transactionsDto
        );

        return Ok(dto);
    }


    [HttpGet("{id}/value")]
    public async Task<ActionResult<PortfolioDto>> GetValue(Guid id, [FromQuery] string currency = "PLN")
    {
        var portfolio = await GetUserPortfolioAsync(id);
        if (portfolio is null) return NotFound();

        decimal convertedAmount = await _currencyConverter.Convert(portfolio.Balance.Amount, portfolio.Balance.Currency, currency);

        return Ok(new PortfolioValueDto(
            portfolio.Id,
            portfolio.Balance.Amount,
            portfolio.Balance.Currency,
            convertedAmount,
            currency.ToUpper()
        ));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePortfolioDto dto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                           User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(userIdString, out var ownerId))
        {
            return Unauthorized("The user could not be identified");
        }

        var portfolio = new Portfolio(dto.Name, ownerId, dto.Currency);


        _dbContext.Portfolios.Add(portfolio);

        await _dbContext.SaveChangesAsync();

        var responseDto = new PortfolioDto
        (
            portfolio.Id,
            portfolio.Name,
            portfolio.Balance.Amount,
            portfolio.Balance.Currency,
            new List<TransactionDto>()
        );


        return CreatedAtAction(nameof(Get), new { id = portfolio.Id }, responseDto);
    }

    [HttpPost("{id}/deposit")]
    public async Task<IActionResult> Deposit(Guid id, CreateTransactionDto dto)
    {
        var portfolio = await GetUserPortfolioAsync(id);
        if (portfolio is null) return NotFound();

        var money = new Money(dto.Amount, dto.Currency);
        portfolio.Deposit(money);

        await _dbContext.SaveChangesAsync();
        return NoContent();

    }

    [HttpPost("{id}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id, CreateTransactionDto dto)
    {
        var portfolio = await GetUserPortfolioAsync(id);
        if (portfolio is null) return NotFound();

        var money = new Money(dto.Amount, dto.Currency);
        portfolio.Withdraw(money);

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<Portfolio?> GetUserPortfolioAsync(Guid poetfolioId)
    {
        var currentUserId = GetUserId();
        return await _dbContext.Portfolios
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.Id == poetfolioId && p.OwnerId == currentUserId);
    }

    private Guid GetUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                           User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (Guid.TryParse(userIdString, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("Nie można zidentyfikować użytkownika.");
    }
}
