using AutoMapper;
using InvestNaijaAuth.Data;
using InvestNaijaAuth.DTO_s;
using InvestNaijaAuth.Entities;
using InvestNaijaAuth.Services;
using Microsoft.EntityFrameworkCore;

namespace InvestNaijaAuth.Servicies
{ 

    public interface IStockService
    {
        Task GetLiveStocksAsync();
        Task<Stock?> GetStockBySymbolAsync(string symbol);
        Task SaveChangesAsync();

    }
    public class StockService : IStockService
    {

        private readonly InvestNaijaDBContext _context;
        private readonly IMapper _mapper;
        private readonly NGXScraperService _scraperService;

        public StockService(InvestNaijaDBContext context, IMapper mapper , NGXScraperService scraperService)
        {
            _context = context;
            _mapper = mapper;
            _scraperService = scraperService;
        }

        public async Task GetLiveStocksAsync()
        {
            var liveStocks = await _scraperService.GetLiveStocksAsync();

            foreach (var stock in liveStocks)
            {
                var existing = await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == stock.Symbol);

                Console.WriteLine(existing == null
                           ? $"Adding new stock: {stock.Symbol}"
                           : $"Updating stock: {stock.Symbol}, new price: {stock.CurrentPrice}");

                if (existing == null)
                {
                    _context.Stocks.Add(stock);
                }
                else
                {
                    existing.Name = stock.Name;
                    existing.CurrentPrice = stock.CurrentPrice;
                    existing.PreviousClose = stock.PreviousClose;
                    existing.OpeningPrice = stock.OpeningPrice;
                    existing.HighPrice = stock.HighPrice;
                    existing.LowPrice = stock.LowPrice;
                    existing.Change = stock.Change;
                    existing.PercChange = stock.PercChange;
                    existing.CalculateChangePercent = stock.CalculateChangePercent;
                    existing.Trades = stock.Trades;
                    existing.Volume = stock.Volume;
                    existing.Value = stock.Value;
                    existing.Market = stock.Market;
                    existing.Sector = stock.Sector;
                    existing.TradeDate = stock.TradeDate;
                }
            }
            await _context.SaveChangesAsync();
        }
        public async Task<Stock?> GetStockBySymbolAsync(string symbol)
        {
            return await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
