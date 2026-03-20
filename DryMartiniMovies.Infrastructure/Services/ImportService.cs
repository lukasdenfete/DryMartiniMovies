using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using DryMartiniMovies.Infrastructure.Mapping;
using DryMartiniMovies.Core.Import;


namespace DryMartiniMovies.Infrastructure.Services
{
    public class ImportService : IImportService
    {
        public async Task<ImportResultDto> ImportFromCsvAsync(string userId, Stream csvFile)
        {
            var records = await ParseCsvAsync(csvFile);

            //TODO: tmdb lookup och neo4j sparning här sen

            return new ImportResultDto
            {
                ImportedCount = records.Count,
                FailedCount = 0,
                Errors = new List<string>()
            };
        }

        public async Task<List<LetterboxdRecord>> ParseCsvAsync(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            });
            csv.Context.RegisterClassMap<LetterboxdRecordMap>();
            var records = csv.GetRecords<LetterboxdRecord>().ToList();
            return await Task.FromResult(records);
        }
    }
}
