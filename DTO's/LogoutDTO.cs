using System.ComponentModel.DataAnnotations;

namespace InvestNaijaAuth.DTO_s
{
    public class LogoutDTO
    {
        [EmailAddress]
        public required string EmailAddress { get; set; }
        public required string HashedPassword { get; set; }

        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
