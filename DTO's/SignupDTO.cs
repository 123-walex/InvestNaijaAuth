using System.ComponentModel.DataAnnotations;

namespace InvestNaijaAuth.DTO_s
{
    public class SignupDTO
    {
       
        [Required(ErrorMessage = "Username is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
        // regularexpression restricts all characters not allowed in the username 
        public string Username { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string HashedPassword { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Email is required")]
        public string EmailAddress { get; set; }

    }
}
