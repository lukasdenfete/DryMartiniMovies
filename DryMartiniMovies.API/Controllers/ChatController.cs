using DryMartiniMovies.Core.Interfaces;
using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Models;
using Microsoft.AspNetCore.Mvc;



namespace DryMartiniMovies.Api.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService){
            _chatService = chatService;
        }
        [HttpPost("chatresponse")]
        public async Task<IActionResult> GetResponse([FromBody] List<ChatMessageDto> chatHistory)
        {
            var message = chatHistory.Select(m => new ConversationMessage{
                Role = m.Role,
                Content = m.Content
            }).ToList();
            var response = await _chatService.ChatAsync(message);
            return Ok(new { reply = response });
        }

    }

}