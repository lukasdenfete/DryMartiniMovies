using DryMartiniMovies.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DryMartiniMovies.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IConfiguration _config;

        public MovieController(IMovieService movieService, IConfiguration config)
        {
            _movieService = movieService;
            _config = config;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovie(string id)
        {
            var movie = await _movieService.GetMovieAsync(id);
            if (movie == null) return NotFound();
            return Ok(movie);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserMovies()
        {
            var userId = _config["App:DefaultUserId"] ?? "1";
            var movies = await _movieService.GetUserMoviesAsync(userId);
            return Ok(movies);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchTmdb([FromQuery] string title, [FromQuery] int year)
        {
            var movie = await _movieService.SearchTmdbAsync(title, year);
            if (movie == null) return NotFound();
            return Ok(movie);
        }
    }
}
