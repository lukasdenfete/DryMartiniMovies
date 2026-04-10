using System.Diagnostics.CodeAnalysis;
using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.Models;
using DryMartiniMovies.Infrastructure.Neo4j;
using Microsoft.VisualBasic;
using Neo4j.Driver;

namespace DryMartiniMovies.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly Neo4jContext _context;

        public UserRepository(Neo4jContext context)
        {
            _context = context;
        }

        public async Task UpsertAsync(User user)
        {
            await using var session = _context.OpenSession();
            await session.RunAsync(@"
                MERGE (u:User {id: $id})
                SET u.username = $username",
                new { id = user.Id, username = user.Username });
        }

        public async Task AddRatingAsync(string userId, int tmdbId, float rating, DateTime watchedDate)
        {
            await using var session = _context.OpenSession();
            await session.RunAsync(@"
                MATCH (u:User {id: $userId})
                MATCH (m:Movie {tmdbId: $tmdbId})
                MERGE (u)-[r:RATED]->(m)
                SET r.rating = $rating,
                    r.watchedDate = $watchedDate",
                new
                {
                    userId,
                    tmdbId,
                    rating,
                    watchedDate = watchedDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
                });
        }

        public async Task<bool> RemoveRatingAsync(string userId, int tmdbId)
        {
            await using var session = _context.OpenSession();
            var cursor = await session.RunAsync(@"
                MATCH (u:User {id: $userId})
                MATCH (m:Movie {tmdbId: $tmdbId})
                MATCH (u)-[r:RATED]->(m)
                DELETE r",
                new { userId, tmdbId });
            var result = await cursor.ConsumeAsync();
            var count = result.Counters.RelationshipsDeleted;
            return count > 0;
        }
        public Task<User?> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}
