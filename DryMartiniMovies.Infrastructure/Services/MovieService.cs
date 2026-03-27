using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.Models;
using Microsoft.Extensions.Configuration;


namespace DryMartiniMovies.Infrastructure.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly TmdbService _tmdbService;
        private readonly IConfiguration _config;

        public MovieService(IMovieRepository movieRepository, TmdbService tmdbService, IConfiguration config)
        {
            _movieRepository = movieRepository;
            _tmdbService = tmdbService;
            _config = config;
        }

        public async Task<IEnumerable<UserMovie>> GetUserMoviesAsync(string userId)
        {
            return await _movieRepository.GetUserMoviesWithRatingsAsync(userId);
        }

        public async Task<Movie?> SearchTmdbAsync(string title, int year)
        {
            return await _tmdbService.SearchMovieAsync(title, year);
        }
        public async Task<UserMovie?> GetMovieAsync(int tmdbId)
        {
            var userId = _config["App:DefaultUserId"] ?? "1";
            return await _movieRepository.GetUserMovieAsync(userId, tmdbId);
        }
        public async Task<StatsDto> GetUserStatsAsync(string userId)
        {
            return await _movieRepository.GetUserStatsAsync(userId);
        }
        public async Task<IEnumerable<PaceDto>> GetUserPaceAsync(string userId){
            return await _movieRepository.GetUserPaceAsync(userId);
        }
        public async Task<IEnumerable<MovieDto>> GetRecentMoviesAsync(string userId){
            return await _movieRepository.GetRecentMoviesAsync(userId);
        }
    }
}
