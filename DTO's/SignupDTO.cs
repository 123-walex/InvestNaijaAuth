using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace InvestNaijaAuth.DTO_s
{
    [Index(nameof(Username), IsUnique = true)]
    [Index(nameof(EmailAddress), IsUnique = true)]
    [Index(nameof(HashedPassword), IsUnique = true)]
    public class SignupDTO
    {
       
        [Required(ErrorMessage = "Username is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_ ]+$", ErrorMessage = "Username can only contain letters, numbers, underscores, and spaces")]

        public required string Username { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public required string HashedPassword { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Email is required")]
        public required string EmailAddress { get; set; }

    }
}
