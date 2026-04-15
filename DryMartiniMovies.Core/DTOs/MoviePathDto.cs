namespace DryMartiniMovies.Core.DTOs;

public class MoviePathDto
{
    public IEnumerable<PathStepDto> Steps { get; set; }
    public int Length { get; set; }
}