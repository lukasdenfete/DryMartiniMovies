using CsvHelper.Configuration;
using DryMartiniMovies.Core.Import;

namespace DryMartiniMovies.Application.Mapping
{
    public class LetterboxdRecordMap : ClassMap<LetterboxdRecord>
    {
        public LetterboxdRecordMap() 
        {
            Map(m => m.Date).Name("Date");
            Map(m => m.Name).Name("Name");
            Map(m => m.Year).Name("Year");
            Map(m => m.LetterboxdUri).Name("Letterboxd URI");
            Map(m => m.Rating).Name("Rating");
        }
    }
}
