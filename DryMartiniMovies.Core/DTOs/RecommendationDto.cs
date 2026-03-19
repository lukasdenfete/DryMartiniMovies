namespace DryMartiniMovies.Core.DTOs;

public class RecommendationDto
{
    public MovieDto Movie { get; set; } = new();
    public string Explanation { get; set; } = string.Empty;
}