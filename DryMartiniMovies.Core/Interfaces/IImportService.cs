using DryMartiniMovies.Core.DTOs;

namespace DryMartiniMovies.Core.Interfaces;

public interface IImportService
{
    Task<ImportResultDto> ImportFromCsvAsync(string userId, Stream csvFile);
}