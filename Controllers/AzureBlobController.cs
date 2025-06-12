using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using InvestNaijaAuth.Servicies;
using InvestNaijaAuth.DTO_s;
using Azure.Storage.Blobs;

namespace InvestNaijaAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AzureBlobController : Controller
    {
        private readonly AzureBlobService _blobService;
        private readonly ILogger<AzureBlobController> _logger;

        public AzureBlobController(AzureBlobService blobService , ILogger<AzureBlobController> logger)
        {
            _blobService = blobService;
            _logger = logger;
        }

        [HttpPost("UploadVideo")]
        public async Task<IActionResult> UploadVideo(VideoUploadDTO videoUpload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var requestId = HttpContext.TraceIdentifier;
            //GETS THE FILE EXTENSION eg mp4
            var extension = Path.GetExtension(videoUpload.File.FileName);

            // generates a new unique name to prevent name conflicts
            var fileName = $"{Guid.NewGuid()}{extension}";

            _logger.LogInformation("Uploading video , TraceId : {requestId} , RequestedBy : Admin , Title :{Title} ", requestId , videoUpload.Title);

            //Uploads the video to azure blob
            var videoUrl = await _blobService.UploadVideoAsync(videoUpload.File, fileName);

            return Ok(new
            {
                videoUpload.Title,
                Url = videoUrl
            });
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
