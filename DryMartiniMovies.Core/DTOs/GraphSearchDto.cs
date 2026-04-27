using DryMartiniMovies.Core.Enums;

namespace DryMartiniMovies.Core.DTOs;

public class GraphSearchDto
{
    public NodeType Label { get; set; }
    public int TmdbId { get; set; }
    public string Name { get; set; }
}