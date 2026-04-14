using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Models;

namespace DryMartiniMovies.Core.Interfaces;

public interface IMovieRepository
{
    Task<Movie> GetByIdAsync(string id);
    Task UpsertAsync(Movie movie);
    Task<IEnumerable<UserMovie>> GetUserMoviesWithRatingsAsync(string userId);
    Task<UserMovie?> GetUserMovieAsync(string userId, int tmdbId);
    Task<StatsDto> GetUserStatsAsync(string userId);
    Task<IEnumerable<(string Name, int TmdbId, double AvgRating)>> GetFavoriteDirectorsAsync(string userId, int minMovies = 2);
    Task<IEnumerable<(string Name, int TmdbId, double AvgRating)>> GetFavoriteActorsAsync(string userId, int minMovies = 3);
    Task<IEnumerable<(string Name, double AvgRating)>> GetFavoriteGenresAsync(string userId, int minMovies = 5);
    Task<IEnumerable<PaceDto>> GetUserPaceAsync(string userId);
    Task<IEnumerable<MovieDto>> GetRecentMoviesAsync(string userId);
    Task<IEnumerable<UserMovie?>> SearchUserHistoryAsync(string title, string userId);
    Task<IEnumerable<CommonDenominatorDto?>> FindConnectorsAsync(string userId);
}