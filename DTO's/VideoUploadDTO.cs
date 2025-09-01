using System.ComponentModel.DataAnnotations;
using InvestNaijaAuth.Enums;

namespace InvestNaijaAuth.DTO_s
{
    public class VideoUploadDTO
    {
       [Required]
       public string? Title { get; set; }

       [Required]
       public required IFormFile File { get; set; }
       public string? Description { get; set; }
       public string? Category { get; set; }
       public TimeSpan Duration { get; set; }
       public required VideoLevel Level { get; set; }

    }
}

