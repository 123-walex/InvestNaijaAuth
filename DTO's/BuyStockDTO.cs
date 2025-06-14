using System.ComponentModel.DataAnnotations;

namespace InvestNaijaAuth.DTO_s
{
    public class BuyStockDTO
    {
       
        [Required]
        public string Symbol { get; set; } = null!;
        [Required]
        public decimal Quantity { get; set; }
        
    }
}
