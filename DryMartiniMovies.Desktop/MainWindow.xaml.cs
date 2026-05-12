using DryMartiniMovies.Desktop.Pages;
using DryMartiniMovies.Desktop.ViewModels;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace DryMartiniMovies.Desktop;

public partial class MainWindow : FluentWindow
{
    public MainWindow(INavigationService navigationService, INavigationViewPageProvider pageProvider, ChatViewModel chatViewModel)
    {
        InitializeComponent();
        DataContext = chatViewModel;
        ChatOverlay.GetResponseAsync = chatViewModel.GetResponseAsync;
        RootNavigationView.SetPageProviderService(pageProvider);
        navigationService.SetNavigationControl(RootNavigationView);
        Loaded += (_, _) => navigationService.Navigate(typeof(ConnectionsPage));
    }
}