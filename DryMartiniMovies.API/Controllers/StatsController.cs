using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DryMartiniMovies.Core.Interfaces;

namespace DryMartiniMovies.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IConfiguration _config;

        public StatsController(IMovieService movieService, IConfiguration config)
        {
            _movieService = movieService;
            _config = config;
        }
        [HttpGet]
        public async Task<IActionResult> GetStats() 
        {
            var userId = _config["App:DefaultUserId"] ?? "1";
            var stats = await _movieService.GetUserStatsAsync(userId);
            return Ok(stats);
        }
    }
}
