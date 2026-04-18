namespace RAGArena.ApiService.Infrastructure.Chunking;

public interface IChunkingService
{
    Task<IReadOnlyList<string>> ChunkAsync(string text, CancellationToken ct = default);
}
