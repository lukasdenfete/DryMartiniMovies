using System;
using System.Collections.Generic;
using System.Text;

namespace DryMartiniMovies.Core.DTOs
{
    public class StatsDto
    {
        public int TotalMovies { get; set; }
        public double AverageRating { get; set; }
        public List<GenreStatDto> TopGenres { get; set; } = new();
        public List<DirectorStatDto> TopDirectors { get; set; } = new();
        public List<RatingDistributionDto> RatingDistribution {  get; set; } = new();
        public List<YearStatDto> MoviesByDecade { get; set; } = new();
    }

    public class GenreStatDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public double AverageRating { get; set; }
    }
    public class DirectorStatDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public double AverageRating { get; set; }
    }
    public class RatingDistributionDto
    {
        public float Rating { get; set; }
        public int Count { get; set; }
    }
    public class YearStatDto
    {
        public int Decade {  get; set; }
        public int Count { get; set; }
    }
}
