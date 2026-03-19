using System;
using DryMartiniMovies.Core;

public class LetterboxdRecord
{
	public DateTime Date { get; set; }
	public string Name { get; set; } = string.Empty;
	public int Year { get; set; }
	public float Rating { get; set; }
	public string LetterboxdUri { get; set; } = string.Empty;
}
