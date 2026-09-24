using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPortfolio.API.Dtos.Assets;
using SmartPortfolio.API.Extensions;
using SmartPortfolio.Application.Portfolios.Commands.BuyAsset;
using SmartPortfolio.Application.Portfolios.Commands.SellAsset;
using SmartPortfolio.Application.Portfolios.Queries.GetPortfolioAssets;

namespace SmartPortfolio.API.Controllers;

[Route("api/portfolios/{portfolioId}/assets")]
[ApiController]
[Authorize]
public class PortfolioAssetsController : ControllerBase
{
    private readonly ISender _sender;

    public PortfolioAssetsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid portfolioId)
    {
        var query = new GetPortfolioAssetsQuery(portfolioId, User.GetUserId());
        var result = await _sender.Send(query);
        return Ok(result);
    }

    [HttpPost("buy")]
    public async Task<IActionResult> Buy(Guid portfolioId, BuyAssetDto dto)
    {
        var command = new BuyAssetCommand(
            portfolioId,
            User.GetUserId(),
            dto.Ticker,
            dto.Quantity,
            dto.PricePerShare,
            dto.PurchaseDate
        );

        await _sender.Send(command);
        return NoContent();
    }

    [HttpPost("sell")]
    public async Task<IActionResult> Sell(Guid portfolioId, SellAssetDto dto)
    {
        var command = new SellAssetCommand(
            portfolioId,
            User.GetUserId(),
            dto.Ticker,
            dto.Quantity,
            dto.SellPricePerShare
        );

        await _sender.Send(command);
        return NoContent();
    }
}