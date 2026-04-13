using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.Models;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace DryMartiniMovies.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly IMovieService _movieService;
        private readonly IRecommendationService _recommendationService;
        private readonly ChatClient _chatClient;
        private readonly string _userId;
        private readonly string _baseDir;
        public ChatService(IMovieService movieService, IRecommendationService recommendationService, ChatClient chatClient, IConfiguration config)
        {
            _movieService = movieService;
            _recommendationService = recommendationService;
            _chatClient = chatClient;
            _userId = config["App:DefaultUserId"] ?? "1";
            _baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AI", "SystemPrompt.md");
        }

        public async Task<string> ChatAsync(List<ConversationMessage> chatHistory)
        {
            var tools = AiTools.GetTools();
            var systemPrompt = File.ReadAllText(_baseDir)
                .Replace("{userId}", _userId);
            var options = new ChatCompletionOptions();
            foreach (var tool in tools){
                options.Tools.Add(tool);
            }

            var messages = chatHistory.Select(m => m.Role switch
            {
                "user" => (ChatMessage)new UserChatMessage(m.Content),
                "assistant" => new AssistantChatMessage(m.Content),
                _ => new SystemChatMessage(m.Content)
            }).TakeLast(5).ToList();
            messages.Insert(0, new SystemChatMessage(systemPrompt));

            var response = await _chatClient.CompleteChatAsync(messages, options);

            //kolla om response innehåller tool calls
            while (response.Value.FinishReason == ChatFinishReason.ToolCalls)
            {
                messages.Add(new AssistantChatMessage(response));
                foreach (var toolCall in response.Value.ToolCalls)
                {
                    string toolResult = "";
                    Console.WriteLine(toolCall.FunctionName);
                    switch (toolCall.FunctionName){

                        case AiTools.GetUserStats:
                            var stats = await _movieService.GetUserStatsAsync(_userId);
                            toolResult = JsonSerializer.Serialize(stats);
                            messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                            break;

                        case AiTools.GetRecentMovies:
                            var recentMovies = await _movieService.GetRecentMoviesAsync(_userId);
                            toolResult = JsonSerializer.Serialize(recentMovies);
                            messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                            break;

                        case AiTools.GetRecommendationsByDirectors:
                            var directorRecs = await _recommendationService.GetByFavoriteDirectorsAsync(_userId);
                            toolResult = JsonSerializer.Serialize(directorRecs);
                            messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                            break;

                        case AiTools.GetRecommendationsByActors:
                            var actorRecs = await _recommendationService.GetByFavoriteActorsAsync(_userId);
                            toolResult = JsonSerializer.Serialize(actorRecs);
                            messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                            break;

                        case AiTools.GetRecommendationsByGenre:
                            var args = JsonDocument.Parse(toolCall.FunctionArguments);
                            var genreName = args.RootElement.TryGetProperty("genreName", out var prop) ? prop.GetString() : null;
                            var genreRecs = await _recommendationService.GetByGenresAsync(_userId, genreName);
                            toolResult = JsonSerializer.Serialize(genreRecs);
                            messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                            break;
                        
                        case AiTools.GetUserPace:
                            var pace = await _movieService.GetUserPaceAsync(_userId);
                            toolResult = JsonSerializer.Serialize(pace);
                            Console.WriteLine(toolResult);
                            messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                            break;
                    }
                }
                response = await _chatClient.CompleteChatAsync(messages, options);
            }

            //ChatCompletion har en property Content som är en lista. Det första elementet har en Text-property.
            //trimmar bort dubbla radbrytningar för att det ska bli snyggare i chattrutan
            return response.Value.Content[0].Text.Replace("\n\n", "\n");
        }
    }
}