using DryMartiniMovies.Core.Models;
using Microsoft.Extensions.Configuration;
using TMDbLib.Client;
using TMDbLib.Objects.Movies;

namespace DryMartiniMovies.Infrastructure.Services
{
    public class TmdbService 
    {
        private readonly TMDbClient _client;

        public TmdbService(IConfiguration config)
        {
            var apiKey = config["Tmdb:ApiKey"];
            _client = new TMDbClient(apiKey);
        }

        public async Task<Core.Models.Movie?> SearchMovieAsync(string title, int year)
        {
            var results = await _client.SearchMovieAsync(title, year: year);
            var match = results.Results.FirstOrDefault();

            if (match == null) return null;

            return await GetMovieDetailsAsync(match.Id, title, year);
        }

        private async Task<Core.Models.Movie> GetMovieDetailsAsync(int tmdbId, string title, int year)
        {
            var details = await _client.GetMovieAsync(tmdbId, MovieMethods.Credits);

            var directors = details.Credits.Crew
                .Where(c => c.Job == "Director")
                .Select(c => new Director { Name = c.Name, TmdbId = c.Id })
                .ToList();

            var actors = details.Credits.Cast
                .Take(10)
                .Select(c => new Actor { Name = c.Name, TmdbId = c.Id })
                .ToList();

            return new Core.Models.Movie
            {
                TmdbId = tmdbId,
                Title = title,
                Year = details.ReleaseDate?.Year ?? year,
                Description = details.Overview,
                PosterPath = details.PosterPath,
                TmdbRating = details.VoteAverage,
                Directors = directors,
                Actors = actors,
                Genres = details.Genres?.Select(g => new Genre { Name = g.Name }).ToList() ?? new List<Genre>()
            };
        }
    }
}
