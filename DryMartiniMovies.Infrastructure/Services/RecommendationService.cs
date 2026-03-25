using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Infrastructure.Services;

namespace DryMartiniMovies.Infrastructure.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly TmdbService _tmdbService;

        public RecommendationService(IMovieRepository movieRepository, TmdbService tmdbService)
        {
            _movieRepository = movieRepository;
            _tmdbService = tmdbService;
        }

        public async Task<IEnumerable<RecommendationDto>> GetByFavoriteDirectorsAsync(string userId, int limit = 10)
        {
            var directors = await _movieRepository.GetFavoriteDirectorsAsync(userId);
            var seenMovies = await _movieRepository.GetUserMoviesWithRatingsAsync(userId);
            var seenTmdbIds = seenMovies.Select(m => m.Movie.TmdbId).ToHashSet();

            var recommendations = new List<RecommendationDto>();

            foreach (var director in directors)
            {
                var movies = await _tmdbService.GetMoviesByDirectorAsync(director.TmdbId);

                var unseen = movies
                    .Where(m => !seenTmdbIds.Contains(m.TmdbId) && m.TmdbRating > 0)
                    .Take(3);

                foreach (var movie in unseen)
                {
                    recommendations.Add(new RecommendationDto
                    {
                        Movie = new MovieDto
                        {
                            Id = movie.TmdbId.ToString(),
                            TmdbId = movie.TmdbId,
                            Title = movie.Title,
                            Year = movie.Year,
                            PosterPath = movie.PosterPath,
                            TmdbRating = movie.TmdbRating,
                            Description = movie.Description,
                            Genres = movie.Genres.Select(g => g.Name).ToList(),
                            Directors = movie.Directors.Select(d => d.Name).ToList(),
                        },
                        Explanation = $"Du har gett {director.Name} {director.AvgRating:F2} ⭐ i snitt"
                    });
                }
            }

            return recommendations
                .GroupBy(r => r.Movie.TmdbId)
                .Select(g => g.First())
                .OrderByDescending(r => r.Movie.TmdbRating)
                .Take(limit);
        }

        public Task<IEnumerable<RecommendationDto>> GetByFavoriteGenresAsync(string userId, int limit = 10) => _movieRepository.GetRecommendationsByGenresAsync(userId, limit);

        public Task<IEnumerable<RecommendationDto>> GetByFavoriteActorsAsync(string userId, int limit = 10) => _movieRepository.GetRecommendationsByActorsAsync(userId, limit);

        public Task<string> ChatAsync(string userId, string message) => throw new NotImplementedException();

    }
}
