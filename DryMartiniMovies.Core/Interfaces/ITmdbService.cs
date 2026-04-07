using DryMartiniMovies.Core.Models;
namespace DryMartiniMovies.Core.Interfaces;

public interface ITmdbService
    {
        List<string> GetGenres();
        Task<List<Core.Models.Movie>> GetMoviesByDirectorAsync(int directorTmdbId);
        Task<List<Core.Models.Movie>> GetMoviesByActorAsync(int actorTmdbId);
        Task<List<Core.Models.Movie>> GetMoviesByGenreAsync(string genreName);
        Task<Core.Models.Movie> GetMovieDetailsAsync(int tmdbId);
        Task<Core.Models.Movie?> SearchMovieAsync(string title, int year);
    }
