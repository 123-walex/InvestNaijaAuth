using System.Collections.Generic;

namespace InvestNaijaAuth.Entities.External.NGX
{
    public class NGXapiResponse
    {
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public required List<StockJsonModel> Data { get; set; }
    }
}
