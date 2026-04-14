using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DryMartiniMovies.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IConfiguration _config;
        private readonly ITmdbService _tmdbService;

        public MoviesController(IMovieService movieService, IConfiguration config, ITmdbService tmdbService)
        {
            _movieService = movieService;
            _config = config;
            _tmdbService = tmdbService;
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
            return Ok(new MovieDto
            {
                Id = movie.TmdbId.ToString(),
                TmdbId = movie.TmdbId,
                Title = movie.Title,
                Year = movie.Year,
                Description = movie.Description,
                PosterPath = movie.PosterPath,
                TmdbRating = movie.TmdbRating,
                Genres = movie.Genres.Select(g => g.Name).ToList(),
                Directors = movie.Directors.Select(d => d.Name).ToList(),
                Actors = movie.Actors.Select(a => a.Name).ToList(),
            });
        }
        [HttpGet("{tmdbId:int}")]
        public async Task<IActionResult> GetMovie(int tmdbId)
        {
            var userId = _config["App:DefaultUserId"] ?? "1";
            var movie = await _movieService.GetMovieAsync(userId, tmdbId);
            if (movie == null) return NotFound();
            return Ok(movie);
        }
        [HttpGet("{tmdbId:int}/details")]
        public async Task<IActionResult> GetMovieDetails(int tmdbId)
        {
            var movie = await _tmdbService.GetMovieDetailsAsync(tmdbId);
            if (movie == null) return NotFound();

            return Ok(new MovieDto
            {
                Id = movie.TmdbId.ToString(),
                TmdbId = movie.TmdbId,
                Title = movie.Title,
                Year = movie.Year,
                Description = movie.Description,
                PosterPath = movie.PosterPath,
                TmdbRating = movie.TmdbRating,
                Genres = movie.Genres.Select(g => g.Name).ToList(),
                Directors = movie.Directors.Select(d => d.Name).ToList(),
                Actors = movie.Actors.Select(a => a.Name).ToList(),
            });
        }
        [HttpGet("pace")]
        public async Task<IActionResult> GetUserPace(){
            var userId = _config["App:DefaultUserId"] ?? "1";
            var pace = await _movieService.GetUserPaceAsync(userId);
            return Ok(pace);
        }
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentMovies(){
            var userId = _config["App:DefaultUserId"] ?? "1";
            var recentMovies = await _movieService.GetRecentMoviesAsync(userId);
            return Ok(recentMovies);
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddMovie([FromBody] AddMovieDto addMovieDto){
            var userId = _config["App:DefaultUserId"] ?? "1";
            var result = await _movieService.AddMovieAsync(addMovieDto.Title, addMovieDto.Year, userId, addMovieDto.UserRating, addMovieDto.WatchedDate);
            if (result == true) 
            {
                return Ok();
            } 
            else 
            {
                return NotFound();
            }
        }
        [HttpDelete("{tmdbId:int}")]
        public async Task<IActionResult> RemoveRating(int tmdbId)
        {
            var userId = _config["App:DefaultUserId"] ?? "1";
            var result = await _movieService.RemoveRatingAsync(userId, tmdbId);
            return Ok(result);
        }
        [HttpGet("history")]
        public async Task<IActionResult> GetUserHistory(string title)
        {
            var userId = _config["App:DefaultUserId"] ?? "1";
            var result = await _movieService.SearchUserHistoryAsync(title, userId);
            return Ok(result);
        }
        [HttpGet("connectors")]
        public async Task<IActionResult> FindConnectors()
        {
            var userId = _config["App:DefaultUserId"] ?? "1";
            var result = await _movieService.FindConnectorsAsync(userId);
            return Ok(result);
        }
    }
}
