using System.Security.Claims;
using AutoMapper;
using InvestNaijaAuth.Data;
using InvestNaijaAuth.DTO_s;
using InvestNaijaAuth.Entities;
using InvestNaijaAuth.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace InvestNaijaAuth.Servicies
{
    public interface IPortfolioService
    {
        Task BuyStockAsync(BuyStockDTO buy);
        Task SellStockAsync(SellStockDTO sell);
        Task<List<PortfolioDTO>> GetUserPortfolioAsync();

    }
    public class PortfolioService : IPortfolioService
    {

        private readonly InvestNaijaDBContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PortfolioService> _logger;

        public PortfolioService(InvestNaijaDBContext context, IMapper mapper , IHttpContextAccessor httpContextAccessor , ILogger<PortfolioService> logger)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task BuyStockAsync( BuyStockDTO buy)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated.");
            var guidUserId = Guid.Parse(userId);

            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == buy.Symbol);
            if (stock == null)
                throw new Exception("Stock not found.");

            var existing = await _context.Portfolios
                .FirstOrDefaultAsync(p => p.UserId == guidUserId && p.StockId == stock.StockId);

            var currentPrice = stock.CurrentPrice;

            if (existing == null)
            {
                var portfolio = new Portfolio
                {
                    PortfolioId = Guid.NewGuid(),
                    UserId = guidUserId,
                    StockId = stock.StockId,
                    Symbol = stock.Symbol,
                    Quantity = buy.Quantity,
                    AverageBuyPrice = (decimal)currentPrice,
                    AveragePrice = (decimal)currentPrice,
                    DateBought = DateTime.UtcNow
                };

                _context.Portfolios.Add(portfolio);
            }
            else
            {
                var totalQuantity = existing.Quantity + buy.Quantity;
                var totalCost = (existing.AveragePrice * existing.Quantity) + (currentPrice * buy.Quantity);

                existing.Quantity = totalQuantity;
                existing.AveragePrice = (decimal)(totalCost / totalQuantity);
                existing.AverageBuyPrice = existing.AveragePrice;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
        public async Task SellStockAsync(SellStockDTO sell)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated.");

            var guidUserId = Guid.Parse(userId);

            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == sell.Symbol);
            if (stock == null)
                throw new Exception("Stock not found.");

            var portfolio = await _context.Portfolios
                .FirstOrDefaultAsync(p => p.UserId == guidUserId && p.StockId == stock.StockId);

            if (portfolio == null)
                throw new Exception("You don't own this stock.");

            if (sell.Quantity <= 0)
                throw new Exception("Sell quantity must be greater than 0.");

            if (portfolio.Quantity < sell.Quantity)
                throw new Exception("You cannot sell more than you own.");

            portfolio.Quantity -= sell.Quantity;

            if (portfolio.Quantity == 0)
            {
                _context.Portfolios.Remove(portfolio);
            }
            else
            {
                portfolio.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
        public async Task<List<PortfolioDTO>> GetUserPortfolioAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated.");

            var guidUserId = Guid.Parse(userId);

            var portfolios = await _context.Portfolios
                .Include(p => p.Stock)
                .Where(p => p.UserId == guidUserId)
                .ToListAsync();

            var portfolioDTOs = portfolios.Select(p => new PortfolioDTO
            {
                PortfolioId = p.PortfolioId,
                Symbol = p.Symbol,
                Name = p.Stock.Name,
                Quantity = p.Quantity,
                AverageBuyPrice = p.AverageBuyPrice,
                CurrentPrice = (decimal)p.Stock.CurrentPrice,
                DateBought = p.DateBought
            }).ToList();

            return portfolioDTOs;
        }
    }
}

