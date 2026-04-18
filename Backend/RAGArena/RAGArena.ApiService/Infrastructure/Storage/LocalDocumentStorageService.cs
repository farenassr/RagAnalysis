namespace RAGArena.ApiService.Infrastructure.Storage;

public class LocalDocumentStorageService(IConfiguration configuration) : IDocumentStorageService
{
    private readonly string _basePath = configuration["Storage:BasePath"]
        ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

    public async Task<string> SaveAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_basePath);

        var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(_basePath, safeFileName);

        await using var destination = File.Create(fullPath);
        await fileStream.CopyToAsync(destination, ct);

        return fullPath;
    }

    public Task<Stream> ReadAsync(string storagePath, CancellationToken ct = default)
    {
        Stream stream = File.OpenRead(storagePath);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        if (File.Exists(storagePath))
            File.Delete(storagePath);

        return Task.CompletedTask;
    }
}
