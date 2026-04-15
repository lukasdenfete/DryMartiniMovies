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
    return new List<ChatTool> { getUserStatsTool, getRecentMoviesTool, getRecommendationsByDirectorsTool, getRecommendationsByActorsTool, getRecommendationsByGenreTool, getUserPaceTool, searchUserHistoryTool };
}
}