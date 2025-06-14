namespace InvestNaijaAuth.DTO_s
{
    public class UpdateStockDTO
    {
        public string Symbol { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal? CurrentPrice { get; set; }
        public decimal? PreviousClose { get; set; }
        public decimal? OpeningPrice { get; set; }
        public decimal? HighPrice { get; set; }
        public decimal? LowPrice { get; set; }
        public decimal? Change { get; set; }
        public decimal? PercChange { get; set; }
        public decimal? CalculateChangePercent { get; set; }
        public int Trades { get; set; }
        public decimal Volume { get; set; }
        public decimal Value { get; set; }
        public string Market { get; set; } = null!;
        public string Sector { get; set; } = null!;
        public DateTime TradeDate { get; set; }
    }

}
