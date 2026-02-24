using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartPortfolio.API.Dtos.Assets;
using SmartPortfolio.API.Extensions;
using SmartPortfolio.Domain.Entities;
using SmartPortfolio.Domain.Interfaces;
using SmartPortfolio.Infrastructure.Persistence;
using System.Security.Claims;

namespace SmartPortfolio.API.Controllers;

[Route("api/portfolios/{portfolioId}/assets")]
[ApiController]
[Authorize]
public class PortfolioAssetsController : ControllerBase
{
    private readonly SmartPortfolioDbContext _dbContext;
    private readonly IStockPricingService _stockPricingService;

    public PortfolioAssetsController(SmartPortfolioDbContext dbContext, IStockPricingService stockPricingService)
    {
        _dbContext = dbContext;
        _stockPricingService = stockPricingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid portfolioId)
    {
        var portfolio = await GetUserPortfolioAsync(portfolioId); 
        if (portfolio is null) return NotFound("Portfolio not found or you don't have access to it.");

        var assetsDto = portfolio.Assets
            .Select(a => new AssetDto(a.Id, a.Ticker, a.TotalQuantity))
            .ToList();

        return Ok(assetsDto);
    }

    [HttpPost("buy")]
    public async Task<IActionResult> Buy(Guid portfolioId, BuyAssetDto dto)
    {
        var portfolio = await GetUserPortfolioAsync(portfolioId);
        if (portfolio is null) return NotFound("Portfolio not found or you don't have access to it."); 
        
        var priceToUse = dto.PricePerShare ?? await _stockPricingService.GetCurrentPriceAsync(dto.Ticker);

        try
        {
            portfolio.BuyAsset(dto.Ticker, dto.Quantity, priceToUse, dto.PurchaseDate);

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
        catch (InvalidOperationException ex) 
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("sell")]
    public async Task<IActionResult> Sell(Guid portfolioId, SellAssetDto dto)
    {
        var portfolio = await GetUserPortfolioAsync(portfolioId);
        if (portfolio is null) return NotFound("Portfolio not found or you don't have access to it.");

        var priceToUse = dto.SellPricePerShare ?? await _stockPricingService.GetCurrentPriceAsync(dto.Ticker);

        try
        {
            portfolio.SellAsset(dto.Ticker, dto.Quantity, priceToUse);

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private async Task<Portfolio?> GetUserPortfolioAsync(Guid portfolioId)
    {
        var userIdString = User.GetUserId();
        return await _dbContext.Portfolios
                               .Include(p => p.Assets)
                                    .ThenInclude(a => a.AssetLots)
                               .Include(p => p.Transactions)
                               .AsSplitQuery()
                               .FirstOrDefaultAsync(p => p.Id == portfolioId && 
                                                         p.OwnerId == userIdString);
    }
}