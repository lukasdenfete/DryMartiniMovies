namespace DryMartiniMovies.Core.Models;

public class UserMovie
{
    public string UserId { get; set; } = string.Empty;
    public string MovieId { get; set; } = string.Empty;
    public float Rating { get; set; }
    public DateTime WatchedDate { get; set; }
    public string? Review { get; set; }
    public Movie? Movie { get; set; }
}