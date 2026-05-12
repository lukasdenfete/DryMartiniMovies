using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DryMartiniMovies.Client.Services;
using DryMartiniMovies.Core.DTOs;

namespace DryMartiniMovies.Desktop.ViewModels;

public class ChatViewModel : INotifyPropertyChanged
{   
    private readonly ChatApiService _chatApiService;
    private string _inputText = string.Empty;
    public string InputText { get => _inputText; set { _inputText = value; OnPropertyChanged(); }}
    public ObservableCollection<ChatMessageDto> Messages { get; } = new ();
    public ChatViewModel(ChatApiService chatApiService)
    {
        _chatApiService = chatApiService;
    }

    public async Task<string> GetResponseAsync(IList<ChatMessageDto> history)
    {
        return await _chatApiService.GetResponse(history.ToList());
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}