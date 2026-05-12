using Wpf.Ui.Abstractions;

namespace DryMartiniMovies.Desktop;

public class ServiceNavigationViewPageProvider(IServiceProvider serviceProvider) : INavigationViewPageProvider
{
    public object? GetPage(Type pageType) => serviceProvider.GetService(pageType);
}
