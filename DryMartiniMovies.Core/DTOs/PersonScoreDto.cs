using DryMartiniMovies.Core.Enums;
using DryMartiniMovies.Core.Models;

namespace DryMartiniMovies.Core.DTOs;

public class PersonScoreDto
{
    public string Name { get; set; }
    public PersonRole Role { get; set; }
    public float Score { get; set; }
    public List<string> MovieTitles { get; set; } 
}