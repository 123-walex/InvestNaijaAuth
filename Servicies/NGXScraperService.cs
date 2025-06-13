using HtmlAgilityPack;
using InvestNaijaAuth.DTO_s;

namespace InvestNaijaAuth.Servicies
{
    public class NGXScraperService
    {
        public async Task<List<StockDTO>> GetLiveStocksAsync()
        {
            var url = "https://ngxgroup.com/market-data/equities-price-list/";
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);

            var stocks = new List<StockDTO>();
            var rows = doc.DocumentNode.SelectNodes("//table//tbody//tr");
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var cells = row.SelectNodes("td");
                    if (cells != null && cells.Count > 3)
                    {
                        var stock = new StockDTO
                        {
                            Symbol = cells[0].InnerText.Trim(),
                            Name = cells[1].InnerText.Trim(),
                            CurrentPrice = decimal.TryParse(cells[2].InnerText.Trim(), out var price) ? price : 0
                        };
                        stocks.Add(stock);
                    }
                }
            }
            return stocks;
        }
    }
}

