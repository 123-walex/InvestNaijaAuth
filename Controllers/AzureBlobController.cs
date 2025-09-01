using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using InvestNaijaAuth.Servicies;
using InvestNaijaAuth.DTO_s;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using InvestNaijaAuth.Enums;
using Microsoft.Extensions.Options;

namespace InvestNaijaAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AzureBlobController : Controller
    {
        private readonly AzureBlobService _blobService;
        private readonly ILogger<AzureBlobController> _logger;
        private readonly AzureBlobSettings _settings;

        public AzureBlobController
            (AzureBlobService blobService , 
            ILogger<AzureBlobController>logger,
            IOptions<AzureBlobSettings> options)
        {
            _blobService = blobService;
            _logger = logger;
            _settings = options.Value;
        }

        [HttpPost("UploadVideo")]
        public async Task<IActionResult> UploadVideo(VideoUploadDTO videoUpload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var requestId = HttpContext.TraceIdentifier;
            var extension = Path.GetExtension(videoUpload.File.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";

            var levelFolder = videoUpload.Level.ToString().ToLower(); // e.g., "beginner"
            var blobPath = $"{levelFolder}/{fileName}"; // full blob path: "beginner/1234abc.mp4"

            _logger.LogInformation("Uploading video , TraceId : {requestId} , RequestedBy : Admin , Title :{Title} , Level: {Level}",
                                    requestId, videoUpload.Title, videoUpload.Level);

            // Upload with full blob path
            var videoUrl = await _blobService.UploadVideoAsync(videoUpload.File, blobPath);

            return Ok(new
            {
                videoUpload.Title,
                videoUpload.Level,
                Url = videoUrl
            });
        }
        [HttpGet("GetVideosByLevel/{level}")]
        public async Task<IActionResult> GetVideosByLevel(string level)
        {
            var videos = await _blobService.GetVideosByLevelAsync(level);

            return Ok(videos);
        }

        [HttpDelete("DeleteVideo/{fileName}")]
        public async Task<IActionResult> DeleteVideo(string fileName)
        {
            var deleted = await _blobService.DeleteAsync(fileName);

            if (!deleted)
                return NotFound(new { message = "File not found or already deleted" });

            return Ok(new { message = "Video deleted successfully" });
        }
       
        [HttpGet("GetAllVideos")]
        public async Task<IActionResult> GetAllVideos()
        {
            var videoUrls = await _blobService.GetAllVideosAsync();
            return Ok(videoUrls);
        }

        [HttpGet("GetVideoByFileName/{fileName}")]
        public async Task<IActionResult> GetVideoById(string fileName)
        {
            var videoUrl = await _blobService.GetVideoByFileNameAsync(fileName);

            if (videoUrl == null)
                return NotFound(new { message = "Video not found" });

            return Ok(new { Url = videoUrl });
        }
       
    }
}
