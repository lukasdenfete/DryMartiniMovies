using DryMartiniMovies.Core.Models;
using DryMartiniMovies.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using TMDbLib.Client;
using TMDbLib.Objects.Discover;
using TMDbLib.Objects.Movies;

namespace DryMartiniMovies.Infrastructure.Services
{
    public class TmdbService : ITmdbService
    {
        private readonly TMDbClient _client;
        private static readonly Dictionary<string, int> GenreIds = new()
        {
            { "Action", 28 },
            { "Adventure", 12 },
            { "Animation", 16 },
            { "Comedy", 35 },
            { "Crime", 80 },
            { "Documentary", 99 },
            { "Drama", 18 },
            { "Family", 10751 },
            { "Fantasy", 14 },
            { "History", 36 },
            { "Horror", 27 },
            { "Music", 10402 },
            { "Mystery", 9648 },
            { "Romance", 10749 },
            { "Science Fiction", 878 },
            { "Thriller", 53 },
            { "War", 10752 },
            { "Western", 37 }
        };

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

            return await GetMovieDetailsAsync(match.Id);
        }

        public async Task<Core.Models.Movie> GetMovieDetailsAsync(int tmdbId)
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
                Title = details.Title,
                Year = details.ReleaseDate?.Year ?? 0,
                Description = details.Overview,
                PosterPath = details.PosterPath,
                TmdbRating = details.VoteAverage,
                Directors = directors,
                Actors = actors,
                Genres = details.Genres?.Select(g => new Genre { Name = g.Name }).ToList() ?? new List<Genre>()
            };
        }

        public async Task<List<Core.Models.Movie>> GetMoviesByDirectorAsync(int directorTmdbId)
        {
            var credits = await _client.GetPersonMovieCreditsAsync(directorTmdbId);

            var directedMovies = credits.Crew
                .Where(m => m.Job == "Director")
                .Take(10)
                .ToList();

            var movies = new List<Core.Models.Movie>();

            foreach (var job in directedMovies)
            {
                var details = await _client.GetMovieAsync(job.Id, MovieMethods.Credits);
                if (details == null || details.VoteCount < 50) continue;

                movies.Add(new Core.Models.Movie
                {
                    TmdbId = details.Id,
                    Title = details.Title,
                    Year = details.ReleaseDate?.Year ?? 0,
                    PosterPath = details.PosterPath,
                    TmdbRating = details.VoteAverage,
                    Description = details.Overview,
                    Genres = details.Genres?.Select(g => new Genre { Name = g.Name }).ToList() ?? new(),
                    Directors = details.Credits.Crew
                        .Where(c => c.Job == "Director")
                        .Select(c => new Director { Name = c.Name, TmdbId = c.Id })
                        .ToList()
                });
            }

            return movies.OrderByDescending(m => m.TmdbRating).ToList();
        }

        public async Task<List<Core.Models.Movie>> GetMoviesByActorAsync(int actorTmdbId)
        {
            var credits = await _client.GetPersonMovieCreditsAsync(actorTmdbId);

            var movies = new List<Core.Models.Movie>();

            foreach (var role in credits.Cast.Take(10))
            {
                var details = await _client.GetMovieAsync(role.Id, MovieMethods.Credits);
                if (details == null || details.VoteCount < 50) continue;

                movies.Add(new Core.Models.Movie
                {
                    TmdbId = details.Id,
                    Title = details.Title,
                    Year = details.ReleaseDate?.Year ?? 0,
                    PosterPath = details.PosterPath,
                    TmdbRating = details.VoteAverage,
                    Description = details.Overview,
                    Genres = details.Genres?.Select(g => new Genre { Name = g.Name }).ToList() ?? new(),
                    Directors = details.Credits.Crew
                        .Where(c => c.Job == "Director")
                        .Select(c => new Director { Name = c.Name, TmdbId = c.Id })
                        .ToList()
                });
            }

            return movies.OrderByDescending(m => m.TmdbRating).ToList();
        }
        public async Task<List<Core.Models.Movie>> GetMoviesByGenreAsync(string genreName)
        {
            if (!GenreIds.TryGetValue(genreName, out var genreId))
                return new List<Core.Models.Movie>();

            var randomPages = Enumerable.Range(1, 10)
                .OrderBy(_ => Random.Shared.Next())
                .Take(3);

            var pages = await Task.WhenAll(randomPages.Select(page =>
                Task.Run(() => _client.DiscoverMoviesAsync()
                    .IncludeWithAllOfGenre(new List<int> { genreId })
                    .OrderBy(DiscoverMovieSortBy.PopularityDesc)
                    .Query(page))));

            return pages
                .Where(p => p?.Results != null)
                .SelectMany(p => p.Results)
                .Where(m => m.VoteCount >= 100 && m.VoteAverage > 0)
                .DistinctBy(m => m.Id)
                .OrderByDescending(m => m.VoteAverage)
                .Select(m => new Core.Models.Movie
                {
                    TmdbId = m.Id,
                    Title = m.Title,
                    Year = m.ReleaseDate?.Year ?? 0,
                    PosterPath = m.PosterPath,
                    TmdbRating = m.VoteAverage,
                }).ToList();
        }
        public List<string> GetGenres()
        {
            var genres = GenreIds.Keys.ToList();
            return genres;
        }
    }
}
