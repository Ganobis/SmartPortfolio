using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPortfolio.API.Dtos.Portfolios;
using SmartPortfolio.API.Dtos.Transactions;
using SmartPortfolio.API.Extensions;
using SmartPortfolio.Application.Portfolios.Commands.CreatePortfolio;
using SmartPortfolio.Application.Portfolios.Commands.DepositFunds;
using SmartPortfolio.Application.Portfolios.Commands.WithdrawFunds;
using SmartPortfolio.Application.Portfolios.Queries.GetPortfolioById;
using SmartPortfolio.Application.Portfolios.Queries.GetPortfolioTransactions;
using SmartPortfolio.Application.Portfolios.Queries.GetPortfolioValue;

namespace SmartPortfolio.API.Controllers;

[Route("api/portfolios")]
[ApiController]
[Authorize]
public class PortfoliosController : ControllerBase
{
    private readonly ISender _sender;

    public PortfoliosController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PortfolioDto>> Get(Guid id)
    {
        var query = new GetPortfolioByIdQuery(id, User.GetUserId());
        var result = await _sender.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}/value")]
    public async Task<ActionResult<PortfolioValueDto>> GetValue(Guid id, [FromQuery] string currency = "PLN")
    {
        var query = new GetPortfolioValueQuery(id, User.GetUserId(), currency);
        var result = await _sender.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid id)
    {
        var query = new GetPortfolioTransactionsQuery(id, User.GetUserId());
        var result = await _sender.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePortfolioDto dto)
    {
        var command = new CreatePortfolioCommand(User.GetUserId(), dto.Name, dto.Currency);
        var responseDto = await _sender.Send(command);

        return CreatedAtAction(nameof(Get), new { id = responseDto.Id }, responseDto);
    }

    [HttpPost("{id}/deposit")]
    public async Task<IActionResult> Deposit(Guid id, CreateTransactionDto dto)
    {
        var command = new DepositFundsCommand(id, User.GetUserId(), dto.Amount, dto.Currency);
        await _sender.Send(command);
        return NoContent();
    }

    [HttpPost("{id}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id, CreateTransactionDto dto)
    {
        var command = new WithdrawFundsCommand(id, User.GetUserId(), dto.Amount, dto.Currency);
        await _sender.Send(command);
        return NoContent();
    }
}