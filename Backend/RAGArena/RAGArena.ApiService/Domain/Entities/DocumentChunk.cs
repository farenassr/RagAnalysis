using Pgvector;

namespace RAGArena.ApiService.Domain.Entities;

public class DocumentChunk
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public Vector Embedding { get; set; } = new(Array.Empty<float>());
    public int TokenCount { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
