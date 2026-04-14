using DryMartiniMovies.Core.Enums;
namespace DryMartiniMovies.Core.DTOs;

public class CommonDenominatorDto
{
    public string Name { get; set; }
    public float Rating { get; set; }
    public IEnumerable<PersonRole> Role { get; set; }
    public string MovieTitle { get; set; }
    
}