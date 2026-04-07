using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.Models;
using DryMartiniMovies.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace DryMartiniMovies.Infrastructure.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly ITmdbService _tmdbService;
        private readonly ILogger<RecommendationService> _logger;

        public RecommendationService(IMovieRepository movieRepository, ITmdbService tmdbService, ILogger<RecommendationService> logger)
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

            return recommendations
                .GroupBy(r => r.Movie.TmdbId)
                .Select(g => g.First())
                .OrderByDescending(r => r.Movie.TmdbRating)
                .Take(limit * 5)
                .OrderBy(_ => Random.Shared.Next())
                .Take(limit);
        }

        public async Task<IEnumerable<RecommendationDto>> GetByGenresAsync(string userId, string? inputGenre, int limit = 10)
        {
            var genreMovies = new List<Movie>();
            var seenMovies = await _movieRepository.GetUserMoviesWithRatingsAsync(userId);
            var seenTmdbIds = seenMovies.Select(m => m.Movie.TmdbId).ToHashSet();

            //om specifik genre angetts, försök hämta såna filmer från TMDB. 
            //returnerar tom lista av tmdbservice om genren inte finns
            if (inputGenre != null)
            {
                 genreMovies = await _tmdbService.GetMoviesByGenreAsync(inputGenre);
            }
            //om ingen genre angavs eller om genren inte hittats i TMDB
            if (!genreMovies.Any())
            {
            var genres = await _movieRepository.GetFavoriteGenresAsync(userId);
            var tasks = genres.Select(genre => _tmdbService.GetMoviesByGenreAsync(genre.Name));
            var results = await Task.WhenAll(tasks);
            var genreList = genres.ToList();
            var recommendations = new List<RecommendationDto>();

            for (int i = 0; i < genreList.Count; i++)
            {
                var genre = genreList[i];
                var unseen = results[i]
                    .Where(m => !seenTmdbIds.Contains(m.TmdbId) && m.TmdbRating >= 7.3)
                    .Take(10);

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
            //deduplicera, slumpa och begränsa resultatet
            return recommendations
                .GroupBy(r => r.Movie.TmdbId)
                .Select(g => g.First())
                .OrderByDescending(r => r.Movie.TmdbRating)
                .Take(limit * 5)
                .OrderBy(_ => Random.Shared.Next())
                .Take(limit);
            }
            else //specifik genre hittades, filtrera osedda filmer
            {
                var unseen = genreMovies.Where(m => !seenTmdbIds.Contains(m.TmdbId) && m.TmdbRating >= 7.3)
                    .Take(10);
                var recommendations = new List<RecommendationDto>();
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
                        Explanation = $"Här är {inputGenre}-filmer med minst 7.3 i betyg."
                    });
                }
            return recommendations
                .GroupBy(r => r.Movie.TmdbId)
                .Select(g => g.First())
                .OrderByDescending(r => r.Movie.TmdbRating)
                .Take(limit * 5)
                .OrderBy(_ => Random.Shared.Next())
                .Take(limit);
            }
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

            return recommendations
                .GroupBy(r => r.Movie.TmdbId)
                .Select(g => g.First())
                .OrderByDescending(r => r.Movie.TmdbRating)
                .Take(limit * 5)
                .OrderBy(_ => Random.Shared.Next())
                .Take(limit);
        }
    }
}