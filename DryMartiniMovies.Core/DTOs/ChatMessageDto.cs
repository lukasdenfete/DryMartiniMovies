namespace DryMartiniMovies.Core.DTOs;

public class ChatMessageDto
{
    public string Role { get; set; } = string.Empty;  // "user" eller "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}