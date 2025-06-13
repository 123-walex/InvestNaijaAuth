namespace InvestNaijaAuth.Entities
{
    public class Portfolio
    {
        public Guid PortfolioId { get; set; }
        public Guid UserId { get; set; }
        public Guid StockId { get; set; }
        public int Quantity { get; set; }
        public decimal AveragePrice { get; set; }
    }
}
