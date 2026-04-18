using RAGArena.ApiService.Domain.Enums;

namespace RAGArena.ApiService.Infrastructure.Embeddings;

public interface IEmbeddingService
{
    Task<float[]> GetEmbeddingAsync(string text, EmbeddingModelType modelType, CancellationToken ct = default);
    Task<IReadOnlyList<float[]>> GetEmbeddingsBatchAsync(IEnumerable<string> texts, EmbeddingModelType modelType, CancellationToken ct = default);
}
