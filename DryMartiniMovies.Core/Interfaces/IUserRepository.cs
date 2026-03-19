namespace DryMartiniMovies.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task UpsertAsync(User user);
}