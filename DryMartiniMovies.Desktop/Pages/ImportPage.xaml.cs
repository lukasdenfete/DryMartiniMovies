using System.Windows.Controls;
using DryMartiniMovies.Desktop.ViewModels;

namespace DryMartiniMovies.Desktop.Pages;

public partial class ImportPage : Page
{
    public ImportPage(ImportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
