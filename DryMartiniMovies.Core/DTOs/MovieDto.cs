namespace DryMartiniMovies.Core.DTOs;

public class MovieDto
{
    public string Id { get; set; } = string.Empty;
    public int TmdbId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Year { get; set; }
    public string? PosterPath { get; set; }
    public double TmdbRating { get; set; }
    public List<string> Genres { get; set; } = new();
    public List<string> Directors { get; set; } = new();
    public List<string> Actors { get; set; } = new();
    public float? UserRating { get; set; }
    public DateTime? WatchedDate { get; set; }
}