using DryMartiniMovies.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace DryMartiniMovies.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IImportService _importService;
        private readonly IConfiguration _config;

        public ImportController(IImportService importService, IConfiguration config)
        {
            _importService = importService;
            _config = config;
        }

        [HttpPost("letterboxd")]
        public async Task<IActionResult> ImportLetterboxd(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            var userId = _config["App:DefaultUserId"] ?? "default";

            using var stream = file.OpenReadStream();
            var result = await _importService.ImportFromCsvAsync(userId, stream);

            return Ok(result);
        }
    }
}
