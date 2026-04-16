using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using DryMartiniMovies.Client.Services;
using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Models;
public class ConnectionsViewModel : INotifyPropertyChanged
{
    private string _movie1;
    public string Movie1 { get => _movie1; set { _movie1 = value; OnPropertyChanged(); if(!_isSelecting)SearchHistory(UserHistory1, Movie1); } }
    private string _movie2;
    public string Movie2 { get => _movie2; set { _movie2 = value; OnPropertyChanged(); if(!_isSelecting)SearchHistory(UserHistory2, Movie2); } }
    private int _tmdbId1;
    public int TmdbId1 { get => _tmdbId1; set { _tmdbId1 = value; OnPropertyChanged(); } }
    private int _tmdbId2;
    public int TmdbId2 { get => _tmdbId2; set { _tmdbId2 = value; OnPropertyChanged(); } }
    private MoviePathDto _moviePathDto;
    public MoviePathDto MoviePathDto { get => _moviePathDto; set { _moviePathDto = value; OnPropertyChanged(); } }
    private string _errorMessage;
    public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }
    private ObservableCollection<UserMovie> _userHistory1 = new ObservableCollection<UserMovie>();
    public ObservableCollection<UserMovie> UserHistory1 { get => _userHistory1; set { _userHistory1 = value; OnPropertyChanged(); } }
    private ObservableCollection<UserMovie> _userHistory2 = new ObservableCollection<UserMovie>();
    public ObservableCollection<UserMovie> UserHistory2 { get => _userHistory2; set { _userHistory2 = value; OnPropertyChanged(); } }
    public event PropertyChangedEventHandler? PropertyChanged;
    private bool _showHistory1;
    public bool ShowHistory1 { get => _showHistory1; set { _showHistory1 = value; OnPropertyChanged(); } }
    private bool _showHistory2; 
    public bool ShowHistory2 { get => _showHistory2; set { _showHistory2 = value; OnPropertyChanged(); }}
    private bool _isSelecting = false;
    private MovieApiService _movieApiService;
    public ICommand FindPathCommand { get; set; }
    public ICommand SaveTmdbId1Command { get; set; }
    public ICommand SaveTmdbId2Command { get; set; }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public ConnectionsViewModel(MovieApiService movieApiService)
    {
        _movieApiService = movieApiService;
        FindPathCommand = new RelayCommand(FindPath);
        SaveTmdbId1Command = new RelayCommand(SaveTmdbId1);
        SaveTmdbId2Command = new RelayCommand(SaveTmdbId2);
    }
    private async void SearchHistory(ObservableCollection<UserMovie> userMovies, string query)
    {
        if (string.IsNullOrEmpty(query)) { userMovies.Clear(); ShowHistory1 = false; ShowHistory2 = false; return; }
        var result = await _movieApiService.SearchUserHistoryAsync(query);
        userMovies.Clear();
        foreach (var r in result)
        {
            userMovies.Add(r);
        }
        if (userMovies == UserHistory1)
        {
            ShowHistory1 = true;
        }
        if (userMovies == UserHistory2)
        {
            ShowHistory2 = true;
        }
    }
    private void SaveTmdbId1(object parameter)
    {
        var userMovie = (UserMovie)parameter;
        TmdbId1 = userMovie.Movie.TmdbId;
        _isSelecting = true;
        Movie1 = userMovie.Movie.Title;
        _isSelecting = false;
        UserHistory1.Clear();
    }
    private void SaveTmdbId2(object parameter)
    {
        var userMovie = (UserMovie)parameter;
        TmdbId2 = userMovie.Movie.TmdbId;
        _isSelecting = true;
        Movie2 = userMovie.Movie.Title;
        _isSelecting = false;
        UserHistory2.Clear();
    }
    private async void FindPath()
    {
        try {
        var path = await _movieApiService.FindShortestPathAsync(TmdbId1, TmdbId2);
        if (path == null)
            ErrorMessage = "Kunde inte hitta någon koppling mellan dessa filmer.";
        else
            MoviePathDto = path;
        } catch(Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}