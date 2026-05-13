using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using DryMartiniMovies.Client.Services;
using DryMartiniMovies.Core.DTOs;
using Microsoft.Win32;

namespace DryMartiniMovies.Desktop.ViewModels;

public class ImportViewModel : INotifyPropertyChanged
{
    private readonly MovieApiService _movieApiService;

    private string? _selectedFilePath;
    public string? SelectedFilePath
    {
        get => _selectedFilePath;
        set { _selectedFilePath = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedFileName)); OnPropertyChanged(nameof(HasFile)); CountRows(); }
    }

    public string? SelectedFileName => _selectedFilePath is null ? null : System.IO.Path.GetFileName(_selectedFilePath);
    public bool HasFile => _selectedFilePath is not null;

    private int _totalRows;
    public int TotalRows
    {
        get => _totalRows;
        set { _totalRows = value; OnPropertyChanged(); }
    }

    private void CountRows()
    {
        if (_selectedFilePath is null) { TotalRows = 0; return; }
        // -1 för header row
        TotalRows = System.IO.File.ReadLines(_selectedFilePath).Count() - 1;
    }

    private bool _isImporting;
    public bool IsImporting
    {
        get => _isImporting;
        set { _isImporting = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowInitial)); OnPropertyChanged(nameof(ShowResult)); }
    }

    private int _seconds;
    public int Seconds
    {
        get => _seconds;
        set { _seconds = value; OnPropertyChanged(); }
    }

    private ImportResultDto? _result;
    public ImportResultDto? Result
    {
        get => _result;
        set { _result = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowInitial)); OnPropertyChanged(nameof(ShowResult)); OnPropertyChanged(nameof(HasErrors)); }
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasErrorMessage)); }
    }

    public bool ShowInitial => !IsImporting && Result is null;
    public bool ShowResult => !IsImporting && Result is not null;
    public bool HasErrors => Result?.Errors.Any() == true;
    public bool HasErrorMessage => ErrorMessage is not null;

    public ICommand SelectFileCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand ResetCommand { get; }

    private Timer? _timer;

    public ImportViewModel(MovieApiService movieApiService)
    {
        _movieApiService = movieApiService;
        SelectFileCommand = new RelayCommand(SelectFile);
        ImportCommand = new RelayCommand(async () => await ImportFile(), () => HasFile && !IsImporting);
        ResetCommand = new RelayCommand(Reset);
    }

    private void SelectFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "CSV-filer (*.csv)|*.csv",
            Title = "Välj Letterboxd ratings.csv"
        };
        if (dialog.ShowDialog() == true)
            SelectedFilePath = dialog.FileName;
    }

    private async Task ImportFile()
    {
        if (SelectedFilePath is null) return;

        IsImporting = true;
        Result = null;
        Seconds = 0;
        ErrorMessage = null;

        _timer = new Timer(_ =>
        {
            Seconds++;
        }, null, 1000, 1000);

        try
        {
            using var content = new MultipartFormDataContent();
            using var stream = System.IO.File.OpenRead(SelectedFilePath);
            using var streamContent = new StreamContent(stream);
            content.Add(streamContent, "file", SelectedFileName!);
            Result = await _movieApiService.ImportLetterboxdAsync(content);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Något gick fel vid import: " + ex.Message;
        }
        finally
        {
            _timer?.Dispose();
            IsImporting = false;
        }
    }

    private void Reset()
    {
        SelectedFilePath = null;
        Result = null;
        Seconds = 0;
        ErrorMessage = null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
