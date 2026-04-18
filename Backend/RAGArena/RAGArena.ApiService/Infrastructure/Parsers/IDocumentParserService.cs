namespace RAGArena.ApiService.Infrastructure.Parsers;

public interface IDocumentParserService
{
    Task<string> ParseAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
