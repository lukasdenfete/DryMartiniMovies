using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.Models;
using System;
using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.Models;
using DryMartiniMovies.Infrastructure.Neo4j;
using Microsoft.Extensions.Logging;

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
        public Task<IEnumerable<Movie>> GetAllMoviesAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<Movie> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
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

            _logger.LogInformation("Sparade i Neo4j: {Title} ({Year}) med {Genres} genres, {Directors} regissörer, {Actors} skådespelare",
                movie.Title,
                movie.Year,
                movie.Genres.Count,
                movie.Directors.Count,
                movie.Actors.Count);
        }

    }
}
