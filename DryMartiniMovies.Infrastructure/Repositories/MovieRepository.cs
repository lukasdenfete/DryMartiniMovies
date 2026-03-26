using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.Models;
using DryMartiniMovies.Infrastructure.Neo4j;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

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
                FOREACH (director IN $directors |
                MERGE (d:Director {name: director.name})
                SET d.tmdbId = director.tmdbId
                MERGE (d)-[:DIRECTED]->(m)
                )
                WITH m
                FOREACH (actor IN $actors |
                    MERGE (a:Actor {name: actor.name})
                    SET a.tmdbId = actor.tmdbId
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
                    directors = movie.Directors.Select(d => new { name = d.Name, tmdbId = d.TmdbId ?? 0 }).ToList(),
                    actors = movie.Actors.Select(a => new { name = a.Name, tmdbId = a.TmdbId ?? 0 }).ToList(),
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
        public async Task<UserMovie?> GetUserMovieAsync(string userId, int tmdbId)
        {
            await using var session = _context.OpenSession();
            var result = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie {tmdbId: $tmdbId})
                OPTIONAL MATCH (m)-[:HAS_GENRE]->(g:Genre)
                OPTIONAL MATCH (d:Director)-[:DIRECTED]->(m)
                OPTIONAL MATCH (a:Actor)-[:ACTED_IN]->(m)
                RETURN m,
                       r.rating AS rating,
                       r.watchedDate AS watchedDate,
                       collect(DISTINCT g.name) AS genres,
                       collect(DISTINCT d.name) AS directors,
                       collect(DISTINCT a.name) AS actors",
                new { userId, tmdbId });

            var records = await result.ToListAsync();
            var record = records.FirstOrDefault();
            if (record == null) return null;

            return new UserMovie
            {
                UserId = userId,
                MovieId = tmdbId.ToString(),
                Rating = record["rating"].As<float>(),
                WatchedDate = DateTime.TryParseExact(
                    record["watchedDate"].As<string>(),
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var date) ? date : DateTime.MinValue,
                Movie = MapMovie(record)
            };
        }
        public async Task<StatsDto> GetUserStatsAsync(string userId)
        {
            await using var session = _context.OpenSession();
            var totalResult = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)
                RETURN count(m) AS total, avg(r.rating) AS avgRating",
            new { userId });
            var totalRecord = await totalResult.SingleAsync();

            // Favoritgenres
            var genreResult = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)-[:HAS_GENRE]->(g:Genre)
                RETURN g.name AS name, count(m) AS count, avg(r.rating) AS avgRating
                ORDER BY count DESC
                LIMIT 10",
                new { userId });
            var genreRecords = await genreResult.ToListAsync();

            // Favoritregissörer
            var directorResult = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)<-[:DIRECTED]-(d:Director)
                RETURN d.name AS name, count(m) AS count, avg(r.rating) AS avgRating
                ORDER BY count DESC",
                new { userId });
            var directorRecords = await directorResult.ToListAsync();

            // Betygsdistribution
            var ratingResult = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)
                RETURN r.rating AS rating, count(m) AS count
                ORDER BY rating",
                new { userId });
            var ratingRecords = await ratingResult.ToListAsync();

            // Filmer per decennium
            var decadeResult = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)
                RETURN (m.year / 10) * 10 AS decade, count(m) AS count
                ORDER BY decade",
                new { userId });
            var decadeRecords = await decadeResult.ToListAsync();

            //Favoritskådespelare
            var actorResult = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)<-[:ACTED_IN]-(a:Actor)
                WITH a.name AS name, count(m) AS count, avg(r.rating) AS avgRating
                WHERE count >= 3
                RETURN name, count, avgRating
                ORDER BY count DESC",
                new { userId });
            var actorRecords = await actorResult.ToListAsync();

            return new StatsDto
            {
                TotalMovies = totalRecord["total"].As<int>(),
                AverageRating = totalRecord["avgRating"].As<double>(),
                TopGenres = genreRecords.Select(r => new GenreStatDto
                {
                    Name = r["name"].As<string>(),
                    Count = r["count"].As<int>(),
                    AverageRating = r["avgRating"].As<double>()
                }).ToList(),
                TopDirectors = directorRecords.Select(r => new DirectorStatDto
                {
                    Name = r["name"].As<string>(),
                    Count = r["count"].As<int>(),
                    AverageRating = r["avgRating"].As<double>()
                }).ToList(),
                RatingDistribution = ratingRecords.Select(r => new RatingDistributionDto
                {
                    Rating = r["rating"].As<float>(),
                    Count = r["count"].As<int>()
                }).ToList(),
                MoviesByDecade = decadeRecords.Select(r => new YearStatDto
                {
                    Decade = r["decade"].As<int>(),
                    Count = r["count"].As<int>()
                }).ToList(),
                TopActors = actorRecords.Select(r => new ActorStatDto
                {
                    Name = r["name"].As<string>(),
                    Count = r["count"].As<int>(),
                    AverageRating = r["avgRating"].As<double>()
                }).ToList()
            };
        }
        public async Task<IEnumerable<RecommendationDto>> GetRecommendationsByDirectorsAsync(string userId, int limit)
        {
            await using var session = _context.OpenSession();

            var result = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(seen:Movie)<-[:DIRECTED]-(d:Director)
                WITH d, avg(r.rating) AS avgRating
                WHERE avgRating >= 3.5
                MATCH (d)-[:DIRECTED]->(rec:Movie)
                WHERE NOT EXISTS {
                    MATCH (:User {id: $userId})-[:RATED]->(rec)
                }
                WITH rec, d, avgRating
                OPTIONAL MATCH (rec)<-[:DIRECTED]-(allDir:Director)
                WITH rec, d, avgRating, collect(DISTINCT allDir.name) AS directors
                OPTIONAL MATCH (rec)-[:HAS_GENRE]->(g:Genre)
                WITH rec, d, avgRating, directors, collect(DISTINCT g.name) AS genres
                RETURN rec, d.name AS directorName, avgRating, directors, genres
                ORDER BY rec.tmdbRating DESC
                LIMIT $limit",
                new { userId, limit });

            var records = await result.ToListAsync();

            return records.Select(record =>
            {
                var node = record["rec"].As<INode>();
                var directorName = record["directorName"].As<string>();
                var avgRating = record["avgRating"].As<double>();
                var directors = record["directors"].As<List<string>>();
                var genres = record["genres"].As<List<string>>();

                return new RecommendationDto
                {
                    Movie = new MovieDto
                    {
                        Id = node["tmdbId"].As<int>().ToString(),
                        TmdbId = node["tmdbId"].As<int>(),
                        Title = node["title"].As<string>(),
                        Description = node["description"].As<string>(),
                        Year = node["year"].As<int>(),
                        PosterPath = node["posterPath"].As<string>(),
                        TmdbRating = node["tmdbRating"].As<double>(),
                        Genres = genres,
                        Directors = directors,
                    },
                    Explanation = $"Du har gett {directorName} {avgRating:F2} i snitt"
                };
            });
        }

        public async Task<IEnumerable<RecommendationDto>> GetRecommendationsByGenresAsync(string userId, int limit)
        {
            await using var session = _context.OpenSession();

            var result = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(seen:Movie)-[:HAS_GENRE]->(g:Genre)
                WITH g, avg(r.rating) AS avgRating, count(seen) AS movieCount
                WHERE avgRating >= 3.5 AND movieCount >= 3
                MATCH (rec:Movie)-[:HAS_GENRE]->(g)
                WHERE NOT EXISTS {
                    MATCH (:User {id: $userId})-[:RATED]->(rec)
                }
                WITH rec, g, avgRating
                OPTIONAL MATCH (rec)<-[:DIRECTED]-(d:Director)
                WITH rec, g, avgRating, collect(DISTINCT d.name) AS directors
                OPTIONAL MATCH (rec)-[:HAS_GENRE]->(allG:Genre)
                WITH rec, g, avgRating, directors, collect(DISTINCT allG.name) AS genres
                RETURN rec, g.name AS genreName, avgRating, directors, genres
                ORDER BY rec.tmdbRating DESC
                LIMIT $limit",
                new { userId, limit });

            var records = await result.ToListAsync();

            return records.Select(record =>
            {
                var node = record["rec"].As<INode>();
                var genreName = record["genreName"].As<string>();
                var avgRating = record["avgRating"].As<double>();
                var directors = record["directors"].As<List<string>>();
                var genres = record["genres"].As<List<string>>();

                return new RecommendationDto
                {
                    Movie = new MovieDto
                    {
                        Id = node["tmdbId"].As<int>().ToString(),
                        TmdbId = node["tmdbId"].As<int>(),
                        Title = node["title"].As<string>(),
                        Description = node["description"].As<string>(),
                        Year = node["year"].As<int>(),
                        PosterPath = node["posterPath"].As<string>(),
                        TmdbRating = node["tmdbRating"].As<double>(),
                        Genres = genres,
                        Directors = directors,
                    },
                    Explanation = $"Du gillar {genreName} – snittbetyg {avgRating:F2} ⭐"
                };
            });
        }

        public async Task<IEnumerable<RecommendationDto>> GetRecommendationsByActorsAsync(string userId, int limit)
        {
            await using var session = _context.OpenSession();

            var result = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(seen:Movie)<-[:ACTED_IN]-(a:Actor)
                WITH a, avg(r.rating) AS avgRating, count(seen) AS movieCount
                WHERE avgRating >= 3.5 AND movieCount >= 3
                MATCH (a)-[:ACTED_IN]->(rec:Movie)
                WHERE NOT EXISTS {
                    MATCH (:User {id: $userId})-[:RATED]->(rec)
                }
                WITH rec, a, avgRating
                OPTIONAL MATCH (rec)<-[:DIRECTED]-(d:Director)
                WITH rec, a, avgRating, collect(DISTINCT d.name) AS directors
                OPTIONAL MATCH (rec)-[:HAS_GENRE]->(g:Genre)
                WITH rec, a, avgRating, directors, collect(DISTINCT g.name) AS genres
                RETURN rec, a.name AS actorName, avgRating, directors, genres
                ORDER BY rec.tmdbRating DESC
                LIMIT $limit",
                new { userId, limit });

            var records = await result.ToListAsync();

            return records.Select(record =>
            {
                var node = record["rec"].As<INode>();
                var actorName = record["actorName"].As<string>();
                var avgRating = record["avgRating"].As<double>();
                var directors = record["directors"].As<List<string>>();
                var genres = record["genres"].As<List<string>>();

                return new RecommendationDto
                {
                    Movie = new MovieDto
                    {
                        Id = node["tmdbId"].As<int>().ToString(),
                        TmdbId = node["tmdbId"].As<int>(),
                        Title = node["title"].As<string>(),
                        Description = node["description"].As<string>(),
                        Year = node["year"].As<int>(),
                        PosterPath = node["posterPath"].As<string>(),
                        TmdbRating = node["tmdbRating"].As<double>(),
                        Genres = genres,
                        Directors = directors,
                    },
                    Explanation = $"Du har gett filmer med {actorName} {avgRating:F2} ⭐ i snitt"
                };
            });
        }
        public async Task<IEnumerable<(string Name, int TmdbId, double AvgRating)>> GetFavoriteDirectorsAsync(string userId, int minMovies = 2)
        {
            await using var session = _context.OpenSession();

            var result = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)<-[:DIRECTED]-(d:Director)
                WHERE d.tmdbId IS NOT NULL
                WITH d, avg(r.rating) AS avgRating, count(m) AS movieCount
                WHERE avgRating >= 3.5 AND movieCount >= $minMovies
                RETURN d.name AS name, d.tmdbId AS tmdbId, avgRating
                ORDER BY avgRating DESC, movieCount DESC
                LIMIT 20",
                new { userId, minMovies });

            var records = await result.ToListAsync();

            return records.Select(r => (
                r["name"].As<string>(),
                r["tmdbId"].As<int>(),
                r["avgRating"].As<double>()
            ));
        }
        public async Task<IEnumerable<(string Name, int TmdbId, double AvgRating)>> GetFavoriteActorsAsync(string userId, int minMovies = 5)
        {
            await using var session = _context.OpenSession();

            var result = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)<-[:ACTED_IN]-(a:Actor)
                WHERE a.tmdbId IS NOT NULL
                WITH a, avg(r.rating) AS avgRating, count(m) AS movieCount
                WHERE avgRating >= 3.5 AND movieCount >= $minMovies
                RETURN a.name AS name, a.tmdbId AS tmdbId, avgRating
                ORDER BY avgRating DESC, movieCount DESC
                LIMIT 20",
                new { userId, minMovies });

            var records = await result.ToListAsync();

            return records.Select(r => (
                r["name"].As<string>(),
                r["tmdbId"].As<int>(),
                r["avgRating"].As<double>()
            ));
        }
        public async Task<IEnumerable<(string Name, double AvgRating)>> GetFavoriteGenresAsync(string userId, int minMovies = 5)
        {
            await using var session = _context.OpenSession();

            var result = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)-[:HAS_GENRE]->(g:Genre)
                WITH g, avg(r.rating) AS avgRating, count(m) AS movieCount
                WHERE avgRating > 3.5 AND movieCount >= $minMovies
                RETURN g.name AS name, avgRating
                ORDER BY avgRating DESC
                LIMIT 5",
                new { userId, minMovies });

            var records = await result.ToListAsync();

            return records.Select(r => (
                r["name"].As<string>(),
                r["avgRating"].As<double>()
            ));
        }
    }
}
