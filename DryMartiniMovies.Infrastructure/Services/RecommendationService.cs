using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace DryMartiniMovies.Infrastructure.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly TmdbService _tmdbService;
        private readonly ILogger<RecommendationService> _logger;

        public RecommendationService(IMovieRepository movieRepository, TmdbService tmdbService, ILogger<RecommendationService> logger)
        {
            _movieRepository = movieRepository;
            _tmdbService = tmdbService;
            _logger = logger;
        }

        public async Task<IEnumerable<RecommendationDto>> GetByFavoriteDirectorsAsync(string userId, int limit = 10)
        {
            var directors = (await _movieRepository.GetFavoriteDirectorsAsync(userId)).ToList();
            var seenMovies = await _movieRepository.GetUserMoviesWithRatingsAsync(userId);
            var seenTmdbIds = seenMovies.Select(m => m.Movie.TmdbId).ToHashSet();

            var tasks = directors.Select(director => _tmdbService.GetMoviesByDirectorAsync(director.TmdbId));
            var results = await Task.WhenAll(tasks);

            var recommendations = new List<RecommendationDto>();

            for (int i = 0; i < directors.Count; i++)
            {
                var director = directors[i];
                var unseen = results[i]
                    .Where(m => !seenTmdbIds.Contains(m.TmdbId) && m.TmdbRating >= 7.0)
                    .Take(5);

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

            var rng = new Random();
            return recommendations
                .GroupBy(r => r.Movie.TmdbId)
                .Select(g => g.First())
                .OrderByDescending(r => r.Movie.TmdbRating)
                .Take(limit * 3)
                .OrderBy(_ => rng.Next())
                .Take(limit);
        }

        public async Task<IEnumerable<RecommendationDto>> GetByFavoriteGenresAsync(string userId, int limit = 10)
        {
            var genres = await _movieRepository.GetFavoriteGenresAsync(userId);
            var seenMovies = await _movieRepository.GetUserMoviesWithRatingsAsync(userId);
            var seenTmdbIds = seenMovies.Select(m => m.Movie.TmdbId).ToHashSet();

            var tasks = genres.Select(genre => _tmdbService.GetMoviesByGenreAsync(genre.Name));
            var results = await Task.WhenAll(tasks);

            var genreList = genres.ToList();
            var recommendations = new List<RecommendationDto>();

            for (int i = 0; i < genreList.Count; i++)
            {
                var genre = genreList[i];
                var unseen = results[i]
                    .Where(m => !seenTmdbIds.Contains(m.TmdbId) && m.TmdbRating >= 7.0)
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
                        Explanation = $"Du gillar {genre.Name} – snittbetyg {genre.AvgRating:F2} ⭐"
                    });
                }
            }

            var rng = new Random();
            return recommendations
                .GroupBy(r => r.Movie.TmdbId)
                .Select(g => g.First())
                .OrderByDescending(r => r.Movie.TmdbRating)
                .Take(limit * 3)
                .OrderBy(_ => rng.Next())
                .Take(limit);
        }

        public async Task<IEnumerable<RecommendationDto>> GetByFavoriteActorsAsync(string userId, int limit = 10)
        {
            var actors = (await _movieRepository.GetFavoriteActorsAsync(userId)).ToList();
            var seenMovies = await _movieRepository.GetUserMoviesWithRatingsAsync(userId);
            var seenTmdbIds = seenMovies.Select(m => m.Movie.TmdbId).ToHashSet();

            var tasks = actors.Select(actor => _tmdbService.GetMoviesByActorAsync(actor.TmdbId));
            var results = await Task.WhenAll(tasks);

            var recommendations = new List<RecommendationDto>();

            for (int i = 0; i < actors.Count; i++)
            {
                var actor = actors[i];
                var unseen = results[i]
                    .Where(m => !seenTmdbIds.Contains(m.TmdbId) && m.TmdbRating >= 7.0)
                    .Take(5);

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
                        Explanation = $"Du har gett filmer med {actor.Name} {actor.AvgRating:F2} ⭐ i snitt"
                    });
                }
            }

            var rng = new Random();
            return recommendations
                .GroupBy(r => r.Movie.TmdbId)
                .Select(g => g.First())
                .OrderByDescending(r => r.Movie.TmdbRating)
                .Take(limit * 3)
                .OrderBy(_ => rng.Next())
                .Take(limit);
        }

        public Task<string> ChatAsync(string userId, string message) => throw new NotImplementedException();
    }
}