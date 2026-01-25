using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartPortfolio.API.Dtos;
using SmartPortfolio.Domain.Entities;
using SmartPortfolio.Infrastructure.Persistence;

namespace SmartPortfolio.API.Controllers
{
    [Route("api/portfolios")]
    [ApiController]
    public class PortfoliosController : ControllerBase
    {
        private readonly SmartPortfolioDbContext _dbContext;

        public PortfoliosController(SmartPortfolioDbContext dbContext)
        {
            _dbContext = dbContext;
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

                return CreatedAtAction(nameof(Create), new { id = portfolio.Id }, portfolio.Id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
