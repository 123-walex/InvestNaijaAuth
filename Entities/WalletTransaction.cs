using InvestNaijaAuth.Enums;

namespace InvestNaijaAuth.Entities
{
    public class WalletTransaction
    {
        public Guid TransactionId { get; set; }
        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; }

        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string? Description { get; set; }

        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    }
}
