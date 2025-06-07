using System;

namespace InvestNaijaAuth.Entities
{
    public class UserSessions
    {
        public Guid Id { get; set; }
        public string? EmailAddress { get; set; }
        public User User { get; set; } = null!;
        public DateTime? LoggedInAt { get; set; }
        public DateTime? LoggedOutAt { get; set; }
        public string? AccessSessionToken { get; set; }
        public string? RefreshSessionToken { get; set; }
    }
}
