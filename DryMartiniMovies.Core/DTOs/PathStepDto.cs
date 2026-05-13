using DryMartiniMovies.Core.Enums;

namespace DryMartiniMovies.Core.DTOs;

public class PathStepDto
{
    public string Name { get; set; }
    public NodeType Type { get; set; }
    public PersonRole Role { get; set; }
}