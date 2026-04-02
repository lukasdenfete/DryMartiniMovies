using DryMartiniMovies.Core.DTOs;

namespace DryMartiniMovies.Core.Interfaces;

public interface IRecommendationService
{
    Task<IEnumerable<RecommendationDto>> GetByFavoriteDirectorsAsync(string userId, int limit = 10);
    Task<IEnumerable<RecommendationDto>> GetByGenresAsync(string userId, string? genreName, int limit = 10);
    Task<IEnumerable<RecommendationDto>> GetByFavoriteActorsAsync(string userId, int limit = 10);
}