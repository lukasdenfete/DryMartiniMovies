using Microsoft.Extensions.Configuration;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace DryMartiniMovies.Infrastructure.Neo4j
{
    public class Neo4jContext : IDisposable
    {
        private readonly IDriver _driver;

        public Neo4jContext(IConfiguration config)
        {
            var uri = config["Neo4j:Uri"];
            var user = config["Neo4j:Username"];
            var password = config["Neo4j:Password"];

            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        }

        public IAsyncSession OpenSession() => _driver.AsyncSession();

        public async Task VerifyConnectivityAsync() => await _driver.VerifyConnectivityAsync();

        public void Dispose() => _driver.Dispose();
    }
}
