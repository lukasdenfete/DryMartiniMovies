using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui;
using Wpf.Ui.Controls;
using DryMartiniMovies.Desktop.ViewModels;

namespace DryMartiniMovies.Desktop.Pages;

public partial class ConnectionsPage : Page
{
    public ConnectionsPage(ConnectionsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}