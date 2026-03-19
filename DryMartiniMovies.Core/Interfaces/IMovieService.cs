namespace DryMartiniMovies.Core.Interfaces;

public interface IMovieService
{
    Task<Movie?> GetMovieAsync(string id);
    Task<IEnumerable<Movie>> GetUserMoviesAsync(string userId);
    Task<Movie?> SearchTmdbAsync(string title, int year);
}