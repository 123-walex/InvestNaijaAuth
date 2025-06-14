using System.Net.Http;
using System.Text.Json;
using InvestNaijaAuth.Entities;
using InvestNaijaAuth.Entities.External.NGX;


namespace InvestNaijaAuth.Services
{
    public class NGXScraperService
    {
        private readonly HttpClient _httpClient;

        public NGXScraperService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Stock>> GetLiveStocksAsync()
        {
            var url = "https://doclib.ngxgroup.com/REST/api/statistics/equities/?market=&sector=&orderby=&pageSize=300&pageNo=0";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return [];

            var content = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<List<StockJsonModel>>(content, options);
            if (result == null) return new List<Stock>();

            return result.Select(s => new Stock
            {
                StockId = Guid.NewGuid(),
                Symbol = s.Symbol,
                Name = s.Company2?.Trim() ?? s.Symbol,
                CurrentPrice = s.ClosePrice ?? 0,
                PreviousClose = s.PrevClosingPrice ?? 0,
                OpeningPrice = s.OpeningPrice ?? 0,
                HighPrice = s.HighPrice,
                LowPrice = s.LowPrice,
                Change = s.Change,
                PercChange = s.PercChange,
                CalculateChangePercent = s.CalculateChangePercent,
                Trades = s.Trades ?? 0,
                Volume = s.Volume ?? 0,
                Value = s.Value ?? 0,
                Market = s.Market,
                Sector = s.Sector,
                TradeDate = s.TradeDate
            }).ToList();
        }
    }
}
