using System;
using DryMartiniMovies.Core;

namespace DryMartiniMovies.Core.Import;

public class LetterboxdRecord
{
    public DateTime Date { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Year { get; set; }

    public string LetterboxdUri { get; set; } = string.Empty;

    public decimal Rating { get; set; }

}
