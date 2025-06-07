using System;

namespace InvestNaijaAuth.Entities
{
    public class RefreshTokens
    {
        public int Guid { get; set; }
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; } = null!;
        public DateTime? Revoked { get; set; }   
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
        public bool IsActive => Revoked == null && !IsExpired;
        public Guid Id { get; set; }
        public User User { get; set; } = null!;
    }
}
