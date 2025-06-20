﻿namespace InvestNaijaAuth.Entities
{
    public class Stock
    {
        public Guid StockId { get; set; } = Guid.NewGuid();
        public required string Symbol { get; set; } = string.Empty;
        public required string Name { get; set; } = string.Empty;
        public decimal? CurrentPrice { get; set; }
        public  decimal? PreviousClose { get; set; }
        public decimal? OpeningPrice { get; set; }
        public decimal? HighPrice { get; set; }
        public decimal? LowPrice { get; set; }
        public decimal? Change { get; set; }
        public decimal? PercChange { get; set; }
        public decimal? CalculateChangePercent { get; set; }
        public int Trades { get; set; }
        public decimal Volume { get; set; }
        public decimal Value { get; set; }
        public required string Market { get; set; } = string.Empty;
        public required string Sector { get; set; } = string.Empty;
        public DateTime TradeDate { get; set; }
    }
}
