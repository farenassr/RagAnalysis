namespace RAGArena.ApiService.Infrastructure.Storage;

public interface IDocumentStorageService
{
    Task<string> SaveAsync(Stream fileStream, string fileName, CancellationToken ct = default);
    Task<Stream> ReadAsync(string storagePath, CancellationToken ct = default);
    Task DeleteAsync(string storagePath, CancellationToken ct = default);
}
