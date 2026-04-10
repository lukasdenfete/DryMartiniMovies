using DryMartiniMovies.Core.Models;

namespace DryMartiniMovies.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task UpsertAsync(User user);
    Task AddRatingAsync(string userId, int tmdbId, float rating, DateTime watchedDate);
    Task<bool> RemoveRatingAsync(string userId, int tmdbId);
}