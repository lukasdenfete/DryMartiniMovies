using DryMartiniMovies.Core.Models;

namespace DryMartiniMovies.Core.Interfaces;

public interface IMovieRepository
{
    Task<Movie> GetByIdAsync(string id);
   // Task<IEnumerable<Movie>> GetAllMoviesAsync(string userId);
    Task UpsertAsync(Movie movie);
    Task<IEnumerable<UserMovie>> GetUserMoviesWithRatingsAsync(string userId);
    Task<UserMovie?> GetUserMovieAsync(string userId, int tmdbId);
}