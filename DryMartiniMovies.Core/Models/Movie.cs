using System;
using DryMartiniMovies.Core;

public class Movie
{
    public string Id { get; set; } = string.Empty;
    public int TmdbId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Year { get; set; }
    public string? PosterPath { get; set; }
    public double TmdbRating { get; set; }
    public List<Genre> Genres { get; set; } = new();
    public List<Director> Directors { get; set; } = new();
    public List<Actor> Actors { get; set; } = new();
}
