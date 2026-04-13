namespace DryMartiniMovies.Core.DTOs;

public class PaceResultDto {
    public IEnumerable<PaceDto> MonthlyPace { get; set; }
    public int TotalCount { get; set; }
}