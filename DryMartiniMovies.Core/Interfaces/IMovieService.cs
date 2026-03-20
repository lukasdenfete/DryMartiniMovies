using DryMartiniMovies.Core.Models;

namespace DryMartiniMovies.Core.Interfaces;

public interface IMovieService
{
    Task<Movie?> GetMovieAsync(string id);
    Task<IEnumerable<UserMovie>> GetUserMoviesAsync(string userId);
    Task<Movie?> SearchTmdbAsync(string title, int year);
}