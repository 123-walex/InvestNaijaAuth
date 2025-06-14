
namespace InvestNaijaAuth.DTO_s
{
    public class PortfolioDTO
    {
            public string Symbol { get; set; } = null!;
            public string Name { get; set; } = null!;
            public decimal Quantity { get; set; }
            public decimal AverageBuyPrice { get; set; }
            public decimal CurrentPrice { get; set; }
            public decimal TotalValue => Quantity * CurrentPrice;
            public decimal UnrealizedProfitOrLoss => (CurrentPrice - AverageBuyPrice) * Quantity;
            public decimal ProfitOrLossPercentage => AverageBuyPrice == 0 ? 0 : ((CurrentPrice - AverageBuyPrice) / AverageBuyPrice) * 100;
            public Guid PortfolioId { get; set; }
            public DateTime DateBought { get; set; }
    }

    }

