using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace InvestNaijaAuth.Entities
{
    public class Wallet
    {
        public Guid WalletId { get; set; } = Guid.NewGuid();
        public decimal AccountBalance { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WalletTransaction> Transactions { get; set; }
        public bool IsDeleted { get; internal set; }
        public decimal TotalRevenue { get; set; }
        public decimal BounceRate { get; set; }


    }
}
