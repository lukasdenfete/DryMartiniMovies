namespace DryMartiniMovies.Core.DTOs;

public class ImportResultDto
{
    public int ImportedCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}