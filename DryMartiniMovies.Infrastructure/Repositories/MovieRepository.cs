using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.Models;
using System;
using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.Models;
using DryMartiniMovies.Infrastructure.Neo4j;

namespace DryMartiniMovies.Infrastructure.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly Neo4jContext _context;
        public Task<IEnumerable<Movie>> GetAllMoviesAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<Movie> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task UpsertAsync(Movie movie)
        {
            throw new NotImplementedException();
        }

    }
}
