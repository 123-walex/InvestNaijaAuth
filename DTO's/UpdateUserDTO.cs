using System.ComponentModel.DataAnnotations;

namespace User_Authapi.DTO_s
{
    public record  UpdateUserDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "UserName is required")]
        public required string UserName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "PassWord is required")]
        [DataType(DataType.Password)]
        public required string HashedPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password is required")]
        [DataType(DataType.EmailAddress)]
        public required string EmailAddress { get; set; }


    }
}
