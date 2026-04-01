using DryMartiniMovies.Core.DTOs;

namespace DryMartiniMovies.Web.Services
{
    public class ChatStateService
    {
        public List<ChatMessageDto> chatHistory { get; set; } = new List<ChatMessageDto>();
        public bool isOpen { get; set; }


        public void ClearHistory(){
            chatHistory.Clear();
        }
    }
}