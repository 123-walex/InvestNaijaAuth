using InvestNaijaAuth.Servicies;
using Microsoft.AspNetCore.Mvc;

namespace InvestNaijaAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : Controller
    {
        private readonly NGXScraperService _scraperService;

        public StockController(NGXScraperService scraperService)
        {
            _scraperService = scraperService;
        }

        [HttpGet("live")]
        public async Task<IActionResult> GetLiveStocks()
        {
            var stocks = await _scraperService.GetLiveStocksAsync();
            return Ok(stocks);
        }
    }
}
