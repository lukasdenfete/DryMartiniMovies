using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Import;
using DryMartiniMovies.Core.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using DryMartiniMovies.Infrastructure.Mapping;
using DryMartiniMovies.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using System.Globalization;


namespace DryMartiniMovies.Infrastructure.Services
{
    public class ImportService : IImportService
    {
        private readonly TmdbService _tmdbService;
        private readonly ILogger<ImportService> _logger;
        private readonly IMovieRepository _movieRepository;
        private readonly IUserRepository _userRepository;

        public ImportService(TmdbService tmdbService, ILogger<ImportService> logger, IMovieRepository movieRepository, IUserRepository userRepository)
        {
            _tmdbService = tmdbService;
            _logger = logger;
            _movieRepository = movieRepository;
            _userRepository = userRepository;
        }

        public async Task<ImportResultDto> ImportFromCsvAsync(string userId, Stream csvFile)
        {
            var records = await ParseCsvAsync(csvFile);
            var errors = new List<string>();
            var imported = 0;
            var total = records.Count;

            _logger.LogInformation("Startar import av {Total} filmer...", total);

            await _userRepository.UpsertAsync(new Core.Models.User { Id = userId, Username = userId });

            foreach (var record in records) 
            {
                var movie = await _tmdbService.SearchMovieAsync(record.Name, record.Year);

                if (movie == null)
                {
                    _logger.LogWarning("Hittades ej i TMDB: {Title} ({Year})", record.Name, record.Year);
                    errors.Add($"Hittades ej i TMDB: {record.Name} ({record.Year})");
                    continue;
                }
                await _movieRepository.UpsertAsync(movie);
                await _userRepository.AddRatingAsync(userId, movie.TmdbId, record.Rating, record.Date);
                imported++;
                _logger.LogInformation("[{Imported}/{Total}] {Title} ({Year})", imported, total, movie.Title, movie.Year);


                await Task.Delay(29);
            }

            _logger.LogInformation("Import klar. {Imported} importerade, {Failed} misslyckades.", imported, errors.Count);


            return new ImportResultDto
            {
                ImportedCount = imported,
                FailedCount = errors.Count,
                Errors = errors
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
