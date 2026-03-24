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
            return await _http.GetFromJsonAsync<UserMovie>($"api/movies/{tmdbId}");
        }
    }
}
