using DryMartiniMovies.Core.Enums;

namespace DryMartiniMovies.Core.Models;

public class Person
{   
    public string Name { get; set; } = string.Empty;
    public int? TmdbId { get; set; }
    public PersonRole Role { get; set; }
}