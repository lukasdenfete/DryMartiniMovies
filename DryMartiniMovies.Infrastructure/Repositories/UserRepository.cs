using DryMartiniMovies.Core;
using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DryMartiniMovies.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        public Task<User?> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task UpsertAsync(User user)
        {
            throw new NotImplementedException();
        }
    }
}
