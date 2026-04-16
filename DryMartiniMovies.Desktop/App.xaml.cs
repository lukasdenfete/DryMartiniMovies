using System.Configuration;
using System.Data;
using System.Windows;
using DryMartiniMovies.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DryMartiniMovies.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        try {
        base.OnStartup(e);
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
  
        _serviceProvider = serviceCollection.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
        }
        catch(Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
        private void ConfigureServices(IServiceCollection services)
    {
        // Configure Logging
        services.AddLogging();

        // Configure HttpClient
        var apiBaseAddress = "http://localhost:5185/";
        services.AddHttpClient<MovieApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseAddress);
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        // Register Services
       // services.AddSingleton<IUserService, UserService>();

        // Register ViewModels
        services.AddTransient<ConnectionsViewModel>();

        // Register Views
        services.AddSingleton<MainWindow>();
    }
}

