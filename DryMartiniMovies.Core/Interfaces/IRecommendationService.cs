using DryMartiniMovies.Core.DTOs;

namespace DryMartiniMovies.Core.Interfaces;

public interface IRecommendationService
{
    Task<string> ChatAsync(string userId, string message);
    Task<IEnumerable<RecommendationDto>> GetByFavoriteDirectorsAsync(string userId, int limit = 10);
    Task<IEnumerable<RecommendationDto>> GetByFavoriteGenresAsync(string userId, int limit = 10);
    Task<IEnumerable<RecommendationDto>> GetByFavoriteActorsAsync(string userId, int limit = 10);
}