using System.Text.Json.Serialization;

namespace InvestNaijaAuth.Entities.External.NGX
{
    public class StockJsonModel
    {
        [JsonPropertyName("$id")]
        public required string Id_ { get; set; }

        public required int Id { get; set; }
        public required string Symbol { get; set; }
        public required string Company2 { get; set; }
        public required decimal? PrevClosingPrice { get; set; }
        public decimal? OpeningPrice { get; set; }
        public decimal? HighPrice { get; set; }
        public decimal? LowPrice { get; set; }
        public decimal? ClosePrice { get; set; }
        public decimal? Change { get; set; }
        public decimal? PercChange { get; set; }
        public decimal? CalculateChangePercent { get; set; }
        public int? Trades { get; set; }
        public decimal? Volume { get; set; }
        public decimal? Value { get; set; }
        public required string Market { get; set; }
        public required string Sector { get; set; }
        public required DateTime TradeDate { get; set; }
    }
}
