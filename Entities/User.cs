namespace InvestNaijaAuth.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public string HashedPassword { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? RestoredAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public decimal WalletBalance { get; set; } = 30000;
    }
}

