using InvestNaijaAuth.Enums;

namespace InvestNaijaAuth.DTO_s
{
    public class DeFundWalletDTO
    {
        public Guid TransactionId { get; set; }
        public Guid WalletId { get; set; }
        public required decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string? Description { get; set; }
        public DateTime PerformedAt { get; set; }
    }
}
