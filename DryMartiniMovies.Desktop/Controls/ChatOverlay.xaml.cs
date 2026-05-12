using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DryMartiniMovies.Core.DTOs;

namespace DryMartiniMovies.Desktop.Controls;

public partial class ChatOverlay : UserControl
{
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(ChatOverlay),
            new PropertyMetadata(false, OnIsOpenChanged));

    public static readonly DependencyProperty IsClosedTooltipVisibleProperty =
        DependencyProperty.Register(nameof(IsClosedTooltipVisible), typeof(bool), typeof(ChatOverlay),
            new PropertyMetadata(true));

    public static readonly DependencyProperty InputTextProperty =
        DependencyProperty.Register(nameof(InputText), typeof(string), typeof(ChatOverlay),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty MessagesProperty =
        DependencyProperty.Register(nameof(Messages), typeof(ObservableCollection<ChatMessageDto>), typeof(ChatOverlay),
            new PropertyMetadata(null));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public bool IsClosedTooltipVisible
    {
        get => (bool)GetValue(IsClosedTooltipVisibleProperty);
        set => SetValue(IsClosedTooltipVisibleProperty, value);
    }

    public string InputText
    {
        get => (string)GetValue(InputTextProperty);
        set => SetValue(InputTextProperty, value);
    }

    public ObservableCollection<ChatMessageDto> Messages
    {
        get => (ObservableCollection<ChatMessageDto>)GetValue(MessagesProperty);
        set => SetValue(MessagesProperty, value);
    }

    // Hook this up to your ChatApiService later
    public Func<IList<ChatMessageDto>, Task<string>>? GetResponseAsync { get; set; }

    public ChatOverlay()
    {
        InitializeComponent();
        Messages = new ObservableCollection<ChatMessageDto>();
        InputBox.KeyDown += InputBox_KeyDown;
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var overlay = (ChatOverlay)d;
        overlay.IsClosedTooltipVisible = !(bool)e.NewValue;
    }

    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        IsOpen = !IsOpen;
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        Messages.Clear();
    }

    private void InputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            _ = SendMessageAsync();
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        _ = SendMessageAsync();
    }

    private async Task SendMessageAsync()
    {
        var text = InputText?.Trim();
        if (string.IsNullOrEmpty(text)) return;

        Messages.Add(new ChatMessageDto { Role = "user", Content = text });
        InputText = string.Empty;
        ScrollToBottom();

        if (GetResponseAsync != null)
        {
            var response = await GetResponseAsync(Messages);
            Messages.Add(new ChatMessageDto { Role = "assistant", Content = response });
            ScrollToBottom();
        }
    }

    private void ScrollToBottom()
    {
        if (MessageList.Items.Count > 0)
            MessageList.ScrollIntoView(MessageList.Items[^1]);
    }
}
