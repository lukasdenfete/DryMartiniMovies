using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DryMartiniMovies.Core.Interfaces;


namespace DryMartiniMovies.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IImportService _importService;

        public ImportController(IImportService importService)
        {
            _importService = importService;
        }

        [HttpPost("letterboxd")]
        public async Task<IActionResult> ImportLetterboxd(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            var result = await _importService.ImportFromCsvAsync("user1", stream);

            return Ok(result);
        }
    }
}
