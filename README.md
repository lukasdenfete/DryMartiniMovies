🍸 Dry Martini Movies

A personal film advisor powered by your Letterboxd history, a graph database, and (soon) a local LLM.


Tech Stack
Backend API - ASP.NET Core Web API (.NET 9)
Database - Neo4j AuraDB
Film data - TMDB API via TMDbLib
Frontend - Blazor Server
LLM (planned) - Ollama (local)
Desktop (planned) - WPF



Running the project
Prerequisites
.NET 9 SDK
A Neo4j AuraDB free instance
A TMDB API key

Add the following to appsettings.Development.json
{
  "Neo4j": {
    "Uri": "neo4j+s://your-instance.databases.neo4j.io",
    "Username": "neo4j",
    "Password": "your-password"
  },
  "Tmdb": {
    "ApiKey": "your-tmdb-api-key"
  },
    "App": {
    "DefaultUserId": "1",
    "DefaultUserName":  "your-name"
  }
}

Start the API
cd DryMartiniMovies.API
dotnet run

Start Blazor
cd DryMartiniMovies.Web
dotnet run

The Blazor app expects the API to be running on https://localhost:5851 by default. Adjust appsettings.json in the Web project if needed.

Then use the import page to upload your ratings.csv file (exported from letterboxd). I have provided an example file as a template that can be used for testing (samples/ratings.csv).

Import pipeline
Letterboxd CSV → TMDB lookup → Neo4j graph. Handles deduplication and provides each film with cast, crew, and genre data.

Architecture
DryMartiniMovies.sln
├── DryMartiniMovies.Core          # Domain models, interfaces
├── DryMartiniMovies.Infrastructure # Repositories, services, Neo4j + TMDB
├── DryMartiniMovies.API           # ASP.NET Core Web API
├── DryMartiniMovies.Web           # Blazor Server frontend
└── DryMartiniMovies.Desktop       # WPF (not started)


Recommendation engine
Three strategies, all filtering against unseen films via TMDB:
By directors — fetches favourite directors from Neo4j (avgRating ≥ 3.5, min. 2 films), pulls their filmographies via TMDB, filters unseen films with tmdbRating ≥ 7.0.
By actors — same pattern (avgRating ≥ 3.5, min. 5 films).
By genres — fetches favourite genres from Neo4j (avgRating > 3.5, min. 5 films), uses TMDB Discover with randomised page sampling, filters at tmdbRating ≥ 7.3.


