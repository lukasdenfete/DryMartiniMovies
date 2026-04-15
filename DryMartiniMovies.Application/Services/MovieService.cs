using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
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
        public async Task<IEnumerable<PersonScoreDto?>> FindConnectorsAsync(string userId)
        {
            var connectors = await _movieRepository.FindConnectorsAsync(userId);
            var grouped = connectors.GroupBy(x => new { x.Name, Role = x.Role.First() }).Where(g => g.Count() > 2).Select(g => 
                new PersonScoreDto
            {
                Name = g.Key.Name,
                Role = g.Key.Role,
                Score = g.Average(x => x.Rating) + (g.Key.Role == Core.Enums.PersonRole.Director ? 0.5f : 0f), //lägger till +0.5 på betyg om det är director då de ska skattas högre
                MovieTitles = g.Select(x => x.MovieTitle).ToList()
            }).OrderByDescending(x => x.Score).Where(x => x.Score > 4);
            return grouped;
        }
        public async Task<IEnumerable<PathStepDto>> FindShortestPathAsync(int tmdbId1, int tmdbId2)
        {
            var path = await _movieRepository.FindShortestPathAsync(tmdbId1, tmdbId2);
            if (!path.Any())
            {
                throw new InvalidOperationException("No path found between the two movies.");
            } else
            {
                return path;
            }
        }
    }
}
