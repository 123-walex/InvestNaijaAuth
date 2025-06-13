using InvestNaijaAuth.Enums;

namespace InvestNaijaAuth.Entities
{
    public class StockTransaction
    {
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
        public Guid StockId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public TradeType Type { get; set; } 
        public DateTime PerformedAt { get; set; }
    }
}
