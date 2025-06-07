using System.ComponentModel.DataAnnotations;

namespace InvestNaijaAuth.DTO_s
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        public string? EmailAddress { get; set; }
        [Required]
        public string? HashedPassword{ get; set; }
    }
}
