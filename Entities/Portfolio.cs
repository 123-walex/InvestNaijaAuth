using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InvestNaijaAuth.Entities
{ 
        public class Portfolio
        {
            [Key]
            public Guid PortfolioId { get; set; }

            [Required]
            public Guid UserId { get; set; }

            [Required]
            public Guid StockId { get; set; }

            [Required]
            public decimal Quantity { get; set; }

            [Required]
            public decimal AverageBuyPrice { get; set; }

            public decimal AveragePrice { get; set; }

            public DateTime DateBought { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? UpdatedAt { get; set; }

            public Stock Stock { get; set; } = null!;
            public User User { get; set; } = null!;
            public string Symbol { get; set; }
    }

    }