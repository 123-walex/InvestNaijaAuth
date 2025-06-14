using InvestNaijaAuth.DTO_s;
using InvestNaijaAuth.Servicies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestNaijaAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PortfolioController : Controller
    {
        private readonly IPortfolioService _portfolioService;

        public PortfolioController(IPortfolioService portfolioService)
        {
            _portfolioService = portfolioService;
        }
        [HttpPost("BuyStocks")]
        public async Task<IActionResult> BuyStock([FromBody] BuyStockDTO buy)
        {
            try
            {
                await _portfolioService.BuyStockAsync(buy);
                return Ok(new { message = "Stock purchased successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("SellStocks")]
        public async Task<IActionResult> SellStock([FromBody] SellStockDTO sell)
        {
            try
            {
                await _portfolioService.SellStockAsync(sell);
                return Ok(new { message = "Stock sold successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("GetUserPortfolio")]
        public async Task<IActionResult> GetUserPortfolio()
        {
            try
            {
                var portfolio = await _portfolioService.GetUserPortfolioAsync();
                return Ok(portfolio);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

