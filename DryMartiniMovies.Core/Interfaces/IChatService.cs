using DryMartiniMovies.Core.Models;
namespace DryMartiniMovies.Core.Interfaces;

public interface IChatService
{
    Task<string> ChatAsync(List<ConversationMessage> chatHistory);
}