using DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HomeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeddingController : Controller
    {
        private readonly DataBaseContext _context;

        public WeddingController(DataBaseContext context)
        {
            this._context = context;
        }
        public class UploadFileRequest
        {
            public List<IFormFile> file { get; set; }
            public string? senderName { get; set; }
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(1073741824)] // 1 GB
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)] // 1 GB
        public async Task<IActionResult> UploadFiles([FromForm] UploadFileRequest request)
        {
            if (request.file == null || request.file.Any(x => x.Length == 0))
                return BadRequest("No file uploaded");

            var uploadPath = Path.Combine("C:\\Wedding", "uploads");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            string newAlbumDirectory = null;
            if (request.senderName != null)
            {
                newAlbumDirectory = Path.Combine(uploadPath, request.senderName);
                Directory.CreateDirectory(newAlbumDirectory);
            }

            uploadPath = newAlbumDirectory ?? uploadPath;
            foreach (var currentFile in request.file)
            {
                var fileName = $"{Guid.NewGuid()}_{currentFile.FileName}";

                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await currentFile.CopyToAsync(stream);
                }
            }

            return Ok(new { message = "File uploaded" });
        }

    }
}
