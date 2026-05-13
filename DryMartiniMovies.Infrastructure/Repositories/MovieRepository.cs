using System.Net;
using System.Xml;
using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Enums;
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

        public MovieRepository(Neo4jContext context)
        {
            _context = context;
        }
        public async Task<Movie> GetByIdAsync(string id)
        {
            await using var session = _context.OpenSession();

            var result = await session.RunAsync(@"
                MATCH (m:Movie {tmdbId: $id})
                OPTIONAL MATCH (m)-[:HAS_GENRE]->(g:Genre)
                OPTIONAL MATCH (d:Person)-[:DIRECTED]->(m)
                OPTIONAL MATCH (a:Person)-[:ACTED_IN]->(m)
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
            var directors = record["directors"].As<List<string>>().Select(d => new Person { Name = d, Role = PersonRole.Director }).ToList();
            var actors = record["actors"].As<List<string>>().Select(a => new Person { Name = a, Role = PersonRole.Actor }).ToList();
            var persons = directors.Concat(actors).ToList();
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
                Persons = persons,
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
                MERGE (p:Person {name: director.name})
                SET p.tmdbId = director.tmdbId
                MERGE (p)-[:DIRECTED]->(m)
                )
                WITH m
                FOREACH (actor IN $actors |
                    MERGE (p:Person {name: actor.name})
                    SET p.tmdbId = actor.tmdbId
                    MERGE (p)-[:ACTED_IN]->(m)
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
                    directors = movie.Persons.Where(p => p.Role.Equals(PersonRole.Director)).Select(p => new { name = p.Name, tmdbId = p.TmdbId ?? 0 }).ToList(),
                    actors =  movie.Persons.Where(p => p.Role.Equals(PersonRole.Actor)).Select(p => new { name = p.Name, tmdbId = p.TmdbId ?? 0 }).ToList()
                });
        }

        public async Task<IEnumerable<UserMovie>> GetUserMoviesWithRatingsAsync(string userId)
        {
            await using var session = _context.OpenSession();
            var result = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)
                OPTIONAL MATCH (m)-[:HAS_GENRE]->(g:Genre)
                OPTIONAL MATCH (d:Person)-[:DIRECTED]->(m)
                OPTIONAL MATCH (a:Person)-[:ACTED_IN]->(m)
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
                WatchedDate = DateTime.TryParseExact(
                    record["watchedDate"].As<string>(),
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var date) ? date : DateTime.MinValue,
                Movie = MapMovie(record)
            });
        }
        public async Task<UserMovie?> GetUserMovieAsync(string userId, int tmdbId)
        {
            await using var session = _context.OpenSession();
            var result = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie {tmdbId: $tmdbId})
                OPTIONAL MATCH (m)-[:HAS_GENRE]->(g:Genre)
                OPTIONAL MATCH (d:Person)-[:DIRECTED]->(m)
                OPTIONAL MATCH (a:Person)-[:ACTED_IN]->(m)
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
        
        public async Task<IEnumerable<UserMovie?>> SearchUserHistoryAsync(string title, string userId)
        {
            await using var session = _context.OpenSession();
            var result = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)
                WHERE toLower(m.title) CONTAINS toLower($title)
                RETURN m,
                    r.rating AS rating,
                    r.watchedDate AS watchedDate
                LIMIT 20",
                new { title, userId });

            var records = await result.ToListAsync();
 
            return records.Select(r => {
                var node = r["m"].As<INode>();
                var movieTitle = node["title"].As<string>();
                var tmdbId = node["tmdbId"].As<int>();
                return new UserMovie
            {
                UserId = userId,
                Movie = new Movie { Title = movieTitle, TmdbId = tmdbId },
                Rating = r["rating"].As<float>(),
                WatchedDate = DateTime.TryParseExact(
                    r["watchedDate"].As<string>(),
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var date) ? date : DateTime.MinValue,
            };});
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
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)<-[:DIRECTED]-(d:Person)
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
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)<-[:ACTED_IN]-(a:Person)
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
        public async Task<IEnumerable<(string Name, int TmdbId, double AvgRating)>> GetFavoriteDirectorsAsync(string userId, int minMovies = 2)
        {
            await using var session = _context.OpenSession();

            var result = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)<-[:DIRECTED]-(d:Person)
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
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)<-[:ACTED_IN]-(a:Person)
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
        public async Task<IEnumerable<PaceDto>> GetUserPaceAsync(string userId){
            await using var session = _context.OpenSession();
            var sinceDate = DateTime.Now.AddMonths(-12).ToString("yyyy-MM-dd");

            var result = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)
                WHERE r.watchedDate >= $sinceDate
                WITH substring(r.watchedDate, 0, 7) AS month, count(m) AS count 
                RETURN month, count
                ORDER BY month",
                new { userId, sinceDate });
            
            var records = await result.ToListAsync();
            return records.Select(r => new PaceDto {
                Count = r["count"].As<int>(),
                WatchedMonth = r["month"].As<string>()
            
            });
        }
        public async Task<IEnumerable<MovieDto>> GetRecentMoviesAsync(string userId){
            await using var session = _context.OpenSession();

            var result = await session.RunAsync(@"
                MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)
                WITH m.title AS title, r.rating AS rating, r.watchedDate AS watchedDate, m.posterPath AS posterPath, m.tmdbId AS tmdbId
                RETURN title, rating, watchedDate, posterPath, tmdbId
                ORDER BY watchedDate DESC
                LIMIT 5",
                new { userId });
            
            var records = await result.ToListAsync();
            return records.Select(r => new MovieDto {
                Title = r["title"].As<string>(),
                TmdbId = r["tmdbId"].As<int>(),
                UserRating = r["rating"].As<float>(),
                PosterPath = r["posterPath"].As<string>(),
                WatchedDate = DateTime.TryParseExact(
                    r["watchedDate"].As<string>(),
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var date) ? date : null
            });     
        }
        private static PathStepDto MapNode(INode node)
        {
            if (node.Labels.Contains("Movie"))
            {
                return new PathStepDto
                {
                    Name = node["title"].As<string>(),
                    Type = NodeType.Movie
                };
            }
            else if (node.Labels.Contains("Person"))
            {
                return new PathStepDto
                {
                    Name = node["name"].As<string>(),
                    Type = NodeType.Person
                };
            }
            else
            {
                throw new InvalidOperationException($"Unknown node label: {string.Join(", ", node.Labels)}");
            }
        }

        public async Task<IEnumerable<PathStepDto>> FindShortestPathAsync(int tmdbId1, int tmdbId2, NodeType label1, NodeType label2)
        {
            await using var session = _context.OpenSession();
            var result = await session.RunAsync(@"
            MATCH (n1 {tmdbId: $tmdbId1}), (n2 {tmdbId: $tmdbId2})
                WHERE $label1 IN labels(n1) AND $label2 IN labels(n2)
            MATCH path = shortestPath((n1)-[:ACTED_IN|DIRECTED*..10]-(n2))
                RETURN path",
            new { tmdbId1, tmdbId2, label1 = label1.ToString(), label2 = label2.ToString() });

            if (await result.FetchAsync())
            {
                 var ipath = result.Current["path"].As<IPath>();
                 return ipath.Nodes.Select(MapNode);
                
            } else
            {
                return Enumerable.Empty<PathStepDto>();
            }
        }
        public async Task<IEnumerable<GraphSearchDto>> SearchGraphAsync(string title, string userId)
        {
            await using var session = _context.OpenSession();
            var result = await session.RunAsync(@"
            MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)
                WHERE toLower(m.title) CONTAINS toLower($title)
                RETURN DISTINCT m.tmdbId AS tmdbId, m.title AS name, 'Movie' AS label
                LIMIT 20
            UNION
            MATCH (u:User {id: $userId})-[r:RATED]->(m:Movie)<-[:ACTED_IN|DIRECTED]-(p:Person)
                WHERE toLower(p.name) CONTAINS toLower($title)
                RETURN DISTINCT p.tmdbId AS tmdbId, p.name AS name, 'Person' AS label
                LIMIT 20",
            new { title, userId });

            var records = await result.ToListAsync();
            return records.Where(r => r["tmdbId"] is not null)
            .Select(r =>  new GraphSearchDto
            {
                Label = Enum.Parse<NodeType>(r["label"].As<string>()),
                TmdbId = r["tmdbId"].As<int>(),
                Name = r["name"].As<string>()
            });
        }
    }
}
