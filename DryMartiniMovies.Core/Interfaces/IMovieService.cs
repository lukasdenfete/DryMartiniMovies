using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Models;

namespace DryMartiniMovies.Core.Interfaces;

public interface IMovieService
{
    Task<IEnumerable<UserMovie>> GetUserMoviesAsync(string userId);
    Task<Movie?> SearchTmdbAsync(string title, int year);
    Task<UserMovie?> GetMovieAsync(int tmdbId);
    Task<StatsDto> GetUserStatsAsync(string userId);
    Task<IEnumerable<PaceDto>> GetUserPaceAsync(string userId);
    Task<IEnumerable<MovieDto>> GetRecentMoviesAsync(string userId);
    Task<bool> AddMovieAsync(string title, int year, string userId, float rating, DateTime watchedDate);
    Task<bool> RemoveRatingAsync(string userId, int tmdbId);
}