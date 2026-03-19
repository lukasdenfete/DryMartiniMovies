namespace DryMartiniMovies.Core;

public class UserMovie
{
    public string UserId { get; set; }
    public string MovieId { get; set; }
    public float Rating { get; set; }
    public DateTime WatchedDate  { get; set; }
    public string? Review { get; set; }
}