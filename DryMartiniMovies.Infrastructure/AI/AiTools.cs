using OpenAI;
using OpenAI.Chat;
public class AiTools {
public const string GetUserStats = "get_user_stats";
public const string GetRecentMovies = "get_recent_movies";
public const string GetRecommendationsByDirectors = "get_recommendations_by_directors";

public const string GetRecommendationsByActors = "get_recommendations_by_actors";

public const string GetRecommendationsByGenre = "get_recommendations_by_genre";
public const string GetUserPace = "get_user_pace";
public const string SearchUserHistory = "search_user_history";
public const string SearchGraph = "search_graph";
public const string FindShortestPath = "find_shortest_path";

public static List<ChatTool> GetTools(){

ChatTool getUserStatsTool = ChatTool.CreateFunctionTool(
    functionName: GetUserStats,
    functionDescription: "Get the stats from a specific user.",
    functionParameters: BinaryData.FromString(@"
    {
        ""type"": ""object"",
        ""properties"": {
            ""userId"": {
                ""type"": ""string"",
                ""description"": ""The unique ID of a specific user.""
            }
        },
        ""required"": [ ""userId"" ]
    }")
);
ChatTool getRecentMoviesTool = ChatTool.CreateFunctionTool(
    functionName: GetRecentMovies,
    functionDescription: "Get recently watched movies from a specific user.",
    functionParameters: BinaryData.FromString(@"
    {
        ""type"": ""object"",
        ""properties"": {
            ""userId"": {
                ""type"": ""string"",
                ""description"": ""The unique ID of a specific user.""
            }
        },
        ""required"": [ ""userId"" ]
    }")
);      
ChatTool getRecommendationsByDirectorsTool = ChatTool.CreateFunctionTool(
    functionName: GetRecommendationsByDirectors,
    functionDescription: "Get recommendations for new movies to watch based on directors the user have rated highly.",
    functionParameters: BinaryData.FromString(@"
    {
        ""type"": ""object"",
        ""properties"": {
            ""userId"": {
                ""type"": ""string"",
                ""description"": ""The unique ID of a specific user.""
            }
        },
        ""required"": [ ""userId"" ]
    }")
);
ChatTool getRecommendationsByActorsTool = ChatTool.CreateFunctionTool(
    functionName: GetRecommendationsByActors,
    functionDescription: "Get recommendations for new movies to watch based on actors the user have rated highly.",
    functionParameters: BinaryData.FromString(@"
    {
        ""type"": ""object"",
        ""properties"": {
            ""userId"": {
                ""type"": ""string"",
                ""description"": ""The unique ID of a specific user.""
            }
        },
        ""required"": [ ""userId"" ]
    }")
);
ChatTool getRecommendationsByGenreTool = ChatTool.CreateFunctionTool(
    functionName: GetRecommendationsByGenre,
    functionDescription: "Get recommendations for new movies to watch based on genre.",
    functionParameters: BinaryData.FromString(@"
    {
        ""type"": ""object"",
        ""properties"": {
            ""userId"": {
                ""type"": ""string"",
                ""description"": ""The unique ID of a specific user.""
            },
            ""genreName"": {
                ""type"": ""string"",
                ""description"": ""The name of a genre""
            }
        },
        ""required"": [ ""userId"" ]
    }")
);
ChatTool getUserPaceTool = ChatTool.CreateFunctionTool(
    functionName: GetUserPace,
    functionDescription: "Get the amount of movies watched per month for the last 12 months for a specific user.",
    functionParameters: BinaryData.FromString(@"
    {
        ""type"": ""object"",
        ""properties"": {
            ""userId"": {
                ""type"": ""string"",
                ""description"": ""The unique ID of a specific user.""
            }
        },
        ""required"": [ ""userId"" ]
    }")
);
ChatTool searchUserHistoryTool = ChatTool.CreateFunctionTool(
    functionName: SearchUserHistory,
    functionDescription: "Searches for a specific movie (by title) in the user's watched movies",
    functionParameters: BinaryData.FromString(@"
    {
        ""type"": ""object"",
        ""properties"": {
            ""title"": {
                ""type"": ""string"",
                ""description"": ""The title of the movie.""
            },
            ""userId"": {
                ""type"": ""string"",
                ""description"": ""The unique ID of a specific user.""
            }
        },
        ""required"": [ ""userId"" ]
    }")
);  
ChatTool searchGraphTool = ChatTool.CreateFunctionTool(
    functionName: SearchGraph,
    functionDescription: "Search the graph database for a movie, director or actor by name. Returns matching nodes with their TmdbId and type (Movie, Director or Actor). Use this to resolve a name to an id before calling find_shortest_path.",
    functionParameters: BinaryData.FromString(@"
    {
        ""type"": ""object"",
        ""properties"": {
            ""title"": {
                ""type"": ""string"",
                ""description"": ""The name of the movie, director or actor to search for.""
            },
            ""userId"": {
                ""type"": ""string"",
                ""description"": ""The unique ID of a specific user.""
            }
        },
        ""required"": [ ""title"", ""userId"" ]
    }")
);

ChatTool findShortestPathTool = ChatTool.CreateFunctionTool(
    functionName: FindShortestPath,
    functionDescription: "Find the shortest connection path between two nodes (movies, directors or actors) in the graph. Returns each step in the path with its name and type. Use search_graph first to get the TmdbId and label for each node.",
    functionParameters: BinaryData.FromString(@"
    {
        ""type"": ""object"",
        ""properties"": {
            ""tmdbId1"": {
                ""type"": ""integer"",
                ""description"": ""The TMDB id of the first node.""
            },
            ""tmdbId2"": {
                ""type"": ""integer"",
                ""description"": ""The TMDB id of the second node.""
            },
            ""label1"": {
                ""type"": ""string"",
                ""enum"": [""Movie"", ""Person""],
                ""description"": ""The node type of the first node.""
            },
            ""label2"": {
                ""type"": ""string"",
                ""enum"": [""Movie"", ""Person""],
                ""description"": ""The node type of the second node.""
            }
        },
        ""required"": [ ""tmdbId1"", ""tmdbId2"", ""label1"", ""label2"" ]
    }")
);

    return new List<ChatTool> { getUserStatsTool, getRecentMoviesTool, getRecommendationsByDirectorsTool, getRecommendationsByActorsTool, getRecommendationsByGenreTool, getUserPaceTool, searchUserHistoryTool, searchGraphTool, findShortestPathTool };
}
}