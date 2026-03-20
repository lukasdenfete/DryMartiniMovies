using DryMartiniMovies.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DryMartiniMovies.Infrastructure.Repositories
{
    public class MovieRepository : IMovieRepository
    {
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
