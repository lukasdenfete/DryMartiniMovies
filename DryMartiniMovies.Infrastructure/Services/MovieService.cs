using DryMartiniMovies.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DryMartiniMovies.Infrastructure.Services
{
    public class MovieService : IMovieService
    {
        public Task<Movie?> GetMovieAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Movie>> GetUserMoviesAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<Movie?> SearchTmdbAsync(string title, int year)
        {
            throw new NotImplementedException();
        }
    }
}
