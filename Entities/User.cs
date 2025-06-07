using Microsoft.EntityFrameworkCore;

namespace InvestNaijaAuth.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Username { get; set; }
        public required string EmailAddress { get; set; }
        public required string HashedPassword { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? RestoredAt { get; set; }
        public bool IsDeleted { get; set; } = false;
       
        [Precision(18, 2)]
        public decimal WalletBalance { get; set; } = 30000;
    }
}

