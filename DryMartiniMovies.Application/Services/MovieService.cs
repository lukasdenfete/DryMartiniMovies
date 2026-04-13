using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.Models;


namespace DryMartiniMovies.Application.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly ITmdbService _tmdbService;
        private readonly IUserRepository _userRepository;

        public MovieService(IMovieRepository movieRepository, ITmdbService tmdbService, IUserRepository userRepository)
        {
            _movieRepository = movieRepository;
            _tmdbService = tmdbService;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserMovie>> GetUserMoviesAsync(string userId)
        {
            return await _movieRepository.GetUserMoviesWithRatingsAsync(userId);
        }

        public async Task<Movie?> SearchTmdbAsync(string title, int year)
        {
            return await _tmdbService.SearchMovieAsync(title, year);
        }
        public async Task<UserMovie?> GetMovieAsync(string userId, int tmdbId)
        {
            return await _movieRepository.GetUserMovieAsync(userId, tmdbId);
        }
        public async Task<StatsDto> GetUserStatsAsync(string userId)
        {
            return await _movieRepository.GetUserStatsAsync(userId);
        }
        public async Task<PaceResultDto> GetUserPaceAsync(string userId){
            var monthlyPace = await _movieRepository.GetUserPaceAsync(userId);
            var total = monthlyPace.Sum(p => p.Count);
            return new PaceResultDto
            {
                MonthlyPace = monthlyPace,
                TotalCount = total
            };
        }
        public async Task<IEnumerable<MovieDto>> GetRecentMoviesAsync(string userId){
            return await _movieRepository.GetRecentMoviesAsync(userId);
        }
        public async Task<bool> RemoveRatingAsync(string userId, int tmdbId)
        {
            return await _userRepository.RemoveRatingAsync(userId, tmdbId);
        }
        public async Task<bool> AddMovieAsync(string title, int year, string userId, float rating, DateTime watchedDate)
        {   
            var movie = await _tmdbService.SearchMovieAsync(title, year);
            if (movie != null) 
            {
                var tmdbId = movie.TmdbId;
                await _movieRepository.UpsertAsync(movie);
                await _userRepository.AddRatingAsync(userId, tmdbId, rating, watchedDate);
                return true;
            }
            else 
            {
                return false;
            }
        }
        public async Task<IEnumerable<UserMovie?>> SearchUserHistoryAsync(string title, string userId)
        {
            return await _movieRepository.SearchUserHistoryAsync(title, userId);
        }
    }
}
