using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.Models;
using System.Runtime.InteropServices;


namespace DryMartiniMovies.Infrastructure.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly TmdbService _tmdbService;

        public MovieService(IMovieRepository movieRepository, TmdbService tmdbService)
        {
            _movieRepository = movieRepository;
            _tmdbService = tmdbService;
        }
        public async Task<Movie?> GetMovieAsync(string id)
        {
            return await _movieRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<UserMovie>> GetUserMoviesAsync(string userId)
        {
            return await _movieRepository.GetUserMoviesWithRatingsAsync(userId);
        }

        public async Task<Movie?> SearchTmdbAsync(string title, int year)
        {
            return await _tmdbService.SearchMovieAsync(title, year);
        }
    }
}
