using System.ComponentModel.DataAnnotations;

namespace InvestNaijaAuth.DTO_s
{
    public class SellStockDTO
    {
        [Required]
        public string Symbol { get; set; } = null!;
        [Required]
        public decimal Quantity { get; set; }

    }
}
