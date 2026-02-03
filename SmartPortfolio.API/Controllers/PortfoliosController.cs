using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartPortfolio.API.Dtos;
using SmartPortfolio.Domain.Entities;
using SmartPortfolio.Infrastructure.Persistence;

namespace SmartPortfolio.API.Controllers;

[Route("api/portfolios")]
[ApiController]
public class PortfoliosController : ControllerBase
{
    private readonly SmartPortfolioDbContext _dbContext;
    public PortfoliosController(SmartPortfolioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PortfolioDto>> Get(Guid id)
    {
        var portfolio = await _dbContext.Portfolios
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (portfolio is null)
        {
            return NotFound();
        }

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

    [HttpPost]
    public async Task<IActionResult> Create(CreatePortfolioDto dto)
    {
        try
        {
            var ownerId = Guid.NewGuid();
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
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/deposit")]
    public async Task<IActionResult> Deposit(Guid id, TransactionDto dto)
    {
        var portfolio = await _dbContext.Portfolios.FindAsync(id);
        if (portfolio is null)
        {
            return NotFound();
        }
        try
        {
            var money = new Money(dto.Amount, dto.Currency);
            portfolio.Deposit(money);

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id, TransactionDto dto)
    {
        var portfolio = await _dbContext.Portfolios.FindAsync(id);
        if (portfolio is null)
        {
            return NotFound();
        }
        try
        {
            var money = new Money(dto.Amount, dto.Currency);
            portfolio.Withdraw(money);

            await _dbContext.SaveChangesAsync();
            return NoContent();

        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message }); ;
        }
    }
}
