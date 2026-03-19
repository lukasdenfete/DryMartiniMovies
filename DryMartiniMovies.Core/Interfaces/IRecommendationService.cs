using DryMartiniMovies.Core.DTOs;

namespace DryMartiniMovies.Core.Interfaces;

public interface IRecommendationService
{
    Task<IEnumerable<RecommendationDto>> GetRecommendationsAsync(string userId);
    Task<string> ChatAsync(string userId, string message);
}