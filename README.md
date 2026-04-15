# 🍸 Dry Martini Movies

A personal film advisor powered by your Letterboxd history, a graph database, and an LLM.

## Tech Stack

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core Web API (.NET 9) |
| Database | Neo4j AuraDB |
| Film data | TMDB API via TMDbLib |
| Frontend | Blazor Server |
| LLM | OpenAi SDK (gpt-4o-mini) |
| Desktop (planned) | WPF |

## Features

### Film library
Browse and search your rated films. Filter by genre, sort by rating or date. Click any film for details — poster, TMDB rating vs your own, cast and crew. Remove ratings directly from the detail view.

### Stats
Overview of your viewing history. Genre distribution, top directors, top actors, decade breakdown, and a hero section highlighting your #1 director and actor by weighted score.

### Pace
Monthly viewing history visualised as a line chart. Shows how your pace has changed over time.

### Recommendations
Three parallel strategies, all filtered against films you haven't seen:
- **By directors** — favourite directors (`avgRating ≥ 3.5`, min. 2 films), filmographies via TMDB, filtered at `tmdbRating ≥ 7.0`
- **By actors** — same pattern (`avgRating ≥ 3.5`, min. 5 films)
- **By genres** — favourite genres (`avgRating > 3.5`, min. 5 films), TMDB Discover with randomised page sampling, filtered at `tmdbRating ≥ 7.3`

### Connections
Find the shortest path between two films in your graph via shared actors and directors. Autocomplete search against your history. Example: *Kill Bill → Uma Thurman → Pulp Fiction*.

### Chat
Floating chat widget powered by GPT-4o mini with tool calling. Available tools: viewing stats, recent films, history search, recommendations by director/actor/genre, and viewing pace (more to come).

### Add films
Search TMDB, confirm details, set rating and watch date, save to graph


## Running the project

### Prerequisites
- .NET 9 SDK
- A [Neo4j AuraDB](https://neo4j.com/cloud/aura/) free instance
- A [TMDB API key](https://developer.themoviedb.org/)
- OpenAI API key

### Configuration

Add the following to `appsettings.Development.json`:

```json
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
    "DefaultUserName": "your-name"
  }
}
```

### Start the API

```bash
cd DryMartiniMovies.API
dotnet run
```

### Start Blazor

```bash
cd DryMartiniMovies.Web
dotnet run
```

The Blazor app expects the API to be running on `https://localhost:5185` by default. Adjust the BaseAddress in `appsettings.json` in the Web project if needed.

Then use the import page to upload your `ratings.csv` file (exported from Letterboxd). A sample file is available in `samples/ratings.csv` for testing.

## Import pipeline

Letterboxd CSV → TMDB lookup → Neo4j graph. Handles deduplication and provides each film with cast, crew, and genre data.

## Architecture

```
DryMartiniMovies.sln
├── DryMartiniMovies.Core           # Domain models, interfaces, DTOs, enums
├── DryMartiniMovies.Application    # MovieService, RecommendationService, ImportService
├── DryMartiniMovies.Infrastructure # Repositories, TmdbService, ChatService, Neo4j
├── DryMartiniMovies.API            # ASP.NET Core Web API
├── DryMartiniMovies.Web            # Blazor Server frontend
└── DryMartiniMovies.Desktop        # WPF (not started)
```

## Graph model

| Node | Properties |
|---|---|
| Movie | tmdbId, title, year |
| User | id, name |
| Actor | tmdbId, name |
| Director | tmdbId, name |
| Genre | name |

Relations: `RATED`, `ACTED_IN`, `DIRECTED`, `HAS_GENRE`
