using InvestNaijaAuth.Enums;

namespace InvestNaijaAuth.DTO_s
{
    public class TradeDTO
    {
        public string Symbol { get; set; } = null!;
        public int Quantity { get; set; }
        public double Price { get; set; }  
        public TradeType Action { get; set; } 
    }
}
