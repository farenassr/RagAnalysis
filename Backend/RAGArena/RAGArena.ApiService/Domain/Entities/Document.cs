using RAGArena.ApiService.Domain.Enums;

namespace RAGArena.ApiService.Domain.Entities;

public class Document
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public DocumentParserType ParserType { get; set; }
    public ChunkingStrategy ChunkingStrategy { get; set; }
    public EmbeddingModelType EmbeddingModel { get; set; }
    public DocumentProcessingStatus Status { get; set; } = DocumentProcessingStatus.Pending;
    public string? ErrorMessage { get; set; }
    public int ChunkCount { get; set; }
    public long ProcessingTimeMs { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public ICollection<DocumentChunk> Chunks { get; set; } = [];
}
