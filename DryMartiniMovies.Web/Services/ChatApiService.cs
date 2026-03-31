using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DryMartiniMovies.Web.Services
{
    public class ChatApiService
    {
        private readonly HttpClient _http;
        private record ChatResponse(string Reply);

        public ChatApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> GetResponse(List<ChatMessageDto> chatHistory)
        {
           var response = await _http.PostAsJsonAsync("api/Chat/chatresponse", chatHistory);
           var result = await response.Content.ReadFromJsonAsync<ChatResponse>();
           return result.Reply;
        }
    }
}