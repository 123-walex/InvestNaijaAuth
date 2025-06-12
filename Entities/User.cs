using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InvestNaijaAuth.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Username { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public required string EmailAddress { get; set; }
        
        [Required]
        [MaxLength(256)]
        public required string HashedPassword { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? RestoredAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<UserSessions> Sessions { get; set; } = new List<UserSessions>();
        public ICollection<RefreshTokens> RefreshTokens { get; set; } = new List<RefreshTokens>();
        public int NoOfSessions { get; set; }

        [Precision(18, 2)]
        public decimal WalletBalance { get; set; } = 30000;
    }
}

