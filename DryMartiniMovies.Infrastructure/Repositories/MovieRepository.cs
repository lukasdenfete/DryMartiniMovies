using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.Models;
using System;
using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.Models;
using DryMartiniMovies.Infrastructure.Neo4j;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using System.Linq;

namespace DryMartiniMovies.Infrastructure.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly Neo4jContext _context;
        private readonly ILogger<MovieRepository> _logger;

        public MovieRepository(Neo4jContext context, ILogger<MovieRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IEnumerable<Movie>> GetAllMoviesAsync(string userId)
        {
            await using var session = _context.OpenSession();

            var result = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[:RATED]->(m:Movie)
                OPTIONAL MATCH (m)-[:HAS_GENRE]->(g:Genre)
                OPTIONAL MATCH (d:Director)-[:DIRECTED]->(m)
                OPTIONAL MATCH (a:Actor)-[:ACTED_IN]->(m)
                RETURN m,
                       collect(DISTINCT g.name) AS genres,
                       collect(DISTINCT d.name) AS directors,
                       collect(DISTINCT a.name) AS actors",
                new { userId });

            var records = await result.ToListAsync();
            return records.Select(MapMovie);
        }

        public async Task<Movie> GetByIdAsync(string id)
        {
            await using var session = _context.OpenSession();

            var result = await session.RunAsync(@"
                MATCH (m:Movie {tmdbId: $id})
                OPTIONAL MATCH (m)-[:HAS_GENRE]->(g:Genre)
                OPTIONAL MATCH (d:Director)-[:DIRECTED]->(m)
                OPTIONAL MATCH (a:Actor)-[:ACTED_IN]->(m)
                RETURN m,
                        collect(DISTINCT g.name) AS genres,
                        collect(DISTINCT d.name) AS directors,
                        collect(DISTINCT a.name) AS actors",
                new { id = int.Parse(id) });

            var record = await result.SingleAsync();
            return MapMovie(record);
        }

        private static Movie MapMovie(IRecord record)
        {
            var node = record["m"].As<INode>();
            var tmdbId = node["tmdbId"].As<int>();

            return new Movie
            {
                Id = tmdbId.ToString(),
                TmdbId = node["tmdbId"].As<int>(),
                Title = node["title"].As<string>(),
                Year = node["year"].As<int>(),
                Description = node["description"].As<string>(),
                PosterPath = node["posterPath"].As<string>(),
                TmdbRating = node["tmdbRating"].As<double>(),
                Genres = record["genres"].As<List<string>>().Select(g => new Genre { Name = g }).ToList(),
                Directors = record["directors"].As<List<string>>().Select(d => new Director { Name = d }).ToList(),
                Actors = record["actors"].As<List<string>>().Select(a => new Actor { Name = a }).ToList()
            };
        }

        public async Task UpsertAsync(Movie movie)
        {
            await using var session = _context.OpenSession();

            await session.RunAsync(@"
                MERGE (m:Movie {tmdbId: $tmdbId})
                SET m.title = $title,
                    m.year = $year,
                    m.description = $description,
                    m.posterPath = $posterPath,
                    m.tmdbRating = $tmdbRating
                
                WITH m
                FOREACH (genreName IN $genres |
                    MERGE (g:Genre {name: genreName})
                    MERGE (m)-[:HAS_GENRE]->(g)
                )
                WITH m 
                FOREACH (directorName IN $directors |
                    MERGE (d:Director {name: directorName})
                    MERGE (d)-[:DIRECTED]->(m)
                )
                WITH m
                FOREACH (actorName IN $actors |
                    MERGE (a:Actor {name: actorName})
                    MERGE (a)-[:ACTED_IN]->(m)
                )",
                new
                {
                    tmdbId = movie.TmdbId,
                    title = movie.Title,
                    year = movie.Year,
                    description = movie.Description ?? "",
                    posterPath = movie.PosterPath ?? "",
                    tmdbRating = movie.TmdbRating,
                    genres = movie.Genres.Select(g => g.Name).ToList(),
                    directors = movie.Directors.Select(d => d.Name).ToList(),
                    actors = movie.Actors.Select(a => a.Name).ToList()
                });
        }

        public async Task<IEnumerable<UserMovie>> GetUserMoviesWithRatingsAsync(string userId)
        {
            await using var session = _context.OpenSession();
            var result = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)
                OPTIONAL MATCH (m)-[:HAS_GENRE]->(g:Genre)
                OPTIONAL MATCH (d:Director)-[:DIRECTED]->(m)
                OPTIONAL MATCH (a:Actor)-[:ACTED_IN]->(m)
                RETURN m,
                       r.rating AS rating,
                       r.watchedDate AS watchedDate,
                       collect(DISTINCT g.name) AS genres,
                       collect(DISTINCT d.name) AS directors,
                       collect(DISTINCT a.name) AS actors",
            new { userId });

            var records = await result.ToListAsync();
            return records.Select(record => new UserMovie
            {
                UserId = userId,
                MovieId = record["m"].As<INode>()["tmdbId"].As<int>().ToString(),
                Rating = record["rating"].As<float>(),
                WatchedDate = DateTime.TryParse(record["watchedDate"].As<string>(), out var date)
                    ? date
                    : DateTime.MinValue,
                Movie = MapMovie(record)
            });
        }

    }
}
