namespace DryMartiniMovies.Core.DTOs;

public class AddMovieDto 
{
    public string Title { get; set; }
    public int Year { get; set; }
    public float UserRating { get; set; }
    public DateTime WatchedDate { get; set; }
}
