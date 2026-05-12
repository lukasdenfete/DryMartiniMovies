using DryMartiniMovies.Desktop.Pages;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace DryMartiniMovies.Desktop;

public partial class MainWindow : FluentWindow
{
    public MainWindow(INavigationService navigationService, INavigationViewPageProvider pageProvider)
    {
        InitializeComponent();
        RootNavigationView.SetPageProviderService(pageProvider);
        navigationService.SetNavigationControl(RootNavigationView);
        Loaded += (_, _) => navigationService.Navigate(typeof(ConnectionsPage));
    }
}