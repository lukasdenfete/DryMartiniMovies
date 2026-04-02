using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DryMartiniMovies.Core.Interfaces;

namespace DryMartiniMovies.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpGet("directors")]
        public async Task<IActionResult> ByDirectors([FromQuery] string userId, [FromQuery] int limit = 10)
        {
            var result = await _recommendationService.GetByFavoriteDirectorsAsync(userId, limit);
            return Ok(result);
        }
        [HttpGet("actors")]
        public async Task<IActionResult> ByActors([FromQuery] string userId, [FromQuery] int limit = 10)
        {
            var result = await _recommendationService.GetByFavoriteActorsAsync(userId, limit);
            return Ok(result);
        }
        [HttpGet("genres")]
        public async Task<IActionResult> ByGenres([FromQuery] string userId, [FromQuery] string? genreName, [FromQuery] int limit = 10)
        {
            var result = await _recommendationService.GetByGenresAsync(userId, genreName, limit);
            return Ok(result);
        }
    }
}
