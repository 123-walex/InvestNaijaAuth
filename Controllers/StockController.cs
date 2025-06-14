using InvestNaijaAuth.Data;
using InvestNaijaAuth.DTO_s;
using InvestNaijaAuth.Entities;
using InvestNaijaAuth.Services;
using InvestNaijaAuth.Servicies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestNaijaAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : Controller
    {
        private readonly NGXScraperService _scraperService;
        private readonly IStockService _stockService;
        private readonly InvestNaijaDBContext _context;

        public StockController(NGXScraperService scraperService , IStockService stockService, InvestNaijaDBContext context)
        {
            _scraperService = scraperService;
            _stockService = stockService;
            _context = context;
        }
        //loads the scraped stocks to the db

        [HttpPost("GetLiveStocks")]
        public async Task<IActionResult> GetLiveStocks()
        {
            await _stockService.GetLiveStocksAsync();
            return Ok("Stocks fetched successfully.");
        }
        [HttpGet("GetAllStocks")]
        public async Task<IActionResult> GetAllStocks()
        {
            var stocks = await _context.Stocks.ToListAsync();
            return Ok(stocks);
        }
        [HttpGet("GetStockBySymbol/{symbol}")]
        public async Task<IActionResult> GetStockBySymbol(string symbol)
        {
            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);

            if (stock == null) 
                return NotFound("Stock not found.");

            return Ok(stock);
        }
        [HttpPut("UpdateStock/{symbol}")]
        public async Task<IActionResult> UpdateStock(string symbol, [FromBody] UpdateStockDTO dto)
        {
            var existing = await _stockService.GetStockBySymbolAsync(symbol);
            if (existing == null)
                return NotFound($"Stock with symbol '{symbol}' not found.");

            existing.Name = dto.Name;
            existing.CurrentPrice = dto.CurrentPrice;
            existing.PreviousClose = dto.PreviousClose;
            existing.OpeningPrice = dto.OpeningPrice;
            existing.HighPrice = dto.HighPrice;
            existing.LowPrice = dto.LowPrice;
            existing.Change = dto.Change;
            existing.PercChange = dto.PercChange;
            existing.CalculateChangePercent = dto.CalculateChangePercent;
            existing.Trades = dto.Trades;
            existing.Volume = dto.Volume;
            existing.Value = dto.Value;
            existing.Market = dto.Market;
            existing.Sector = dto.Sector;
            existing.TradeDate = dto.TradeDate;

            await _stockService.SaveChangesAsync();

            return Ok($"Stock '{symbol}' updated successfully.");
        }

        [HttpDelete("DeleteStock/{symbol}")]
        public async Task<IActionResult> DeleteStock(string symbol)
        {
            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);
            if (stock == null) 
                return NotFound("Stock not found.");

            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();

            return Ok("Stock deleted.");
        }
        [HttpGet("SearchforStock")]
        public async Task<IActionResult> SearchStocks([FromQuery] string query)
        {
            var stocks = await _context.Stocks
                .Where(s => s.Name.Contains(query) || s.Sector.Contains(query))
                .ToListAsync();

            return Ok(stocks);
        }

    }
}
