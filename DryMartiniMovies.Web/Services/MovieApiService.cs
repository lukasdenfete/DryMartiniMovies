using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Models;
using System.Net.Http.Json;

namespace DryMartiniMovies.Web.Services
{
    public class MovieApiService
    {
        private readonly HttpClient _http;

        public MovieApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<UserMovie>> GetUserMoviesAsync()
        {
            return await _http.GetFromJsonAsync<List<UserMovie>>("api/movies/user") ?? new List<UserMovie>();
        }

        public async Task<UserMovie?> GetMovieAsync(int tmdbId)
        {
            var response = await _http.GetAsync($"api/movies/{tmdbId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserMovie>();
        }
        public async Task<List<RecommendationDto>> GetRecommendationsByDirectorsAsync()
        {
            var response = await _http.GetAsync("api/Recommendation/directors?userId=1&limit=60");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<RecommendationDto>>()
                   ?? new List<RecommendationDto>();
        }
        public async Task<List<RecommendationDto>> GetRecommendationsByActorsAsync()
        {
            var response = await _http.GetAsync("api/Recommendation/actors?userId=1&limit=60");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<RecommendationDto>>()
                   ?? new List<RecommendationDto>();
        }
        public async Task<List<RecommendationDto>> GetRecommendationsByGenresAsync()
        {
            var response = await _http.GetAsync("api/Recommendation/genres?userId=1&limit=60");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<RecommendationDto>>()
                   ?? new List<RecommendationDto>();
        }
        public async Task<MovieDto?> GetMovieDtoAsync(int tmdbId)
        {
            return await _http.GetFromJsonAsync<MovieDto>($"api/movies/{tmdbId}/details");
        }
        public async Task<List<PaceDto>> GetUserPaceAsync(){
            return await _http.GetFromJsonAsync<List<PaceDto>>("api/movies/pace") ?? new List<PaceDto>();
        }
    }
}
