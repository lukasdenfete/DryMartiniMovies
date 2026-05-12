using System.Windows;
using DryMartiniMovies.Client.Services;
using DryMartiniMovies.Desktop.Pages;
using DryMartiniMovies.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui;
using Wpf.Ui.Abstractions;

namespace DryMartiniMovies.Desktop;

public partial class App : Application
{
    private IServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();

        var apiBaseAddress = "http://localhost:5185/";
        services.AddHttpClient<MovieApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseAddress);
            client.Timeout = TimeSpan.FromMinutes(10);
        });
        services.AddHttpClient<ChatApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseAddress);
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        // WPF-UI navigation
        services.AddSingleton<INavigationViewPageProvider, ServiceNavigationViewPageProvider>();
        services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels
        services.AddTransient<ConnectionsViewModel>();
        services.AddTransient<ImportViewModel>();
        services.AddSingleton<ChatViewModel>();

        // Pages
        services.AddTransient<ConnectionsPage>();
        services.AddTransient<ImportPage>();

        // Main window
        services.AddSingleton<MainWindow>();
    }
}
