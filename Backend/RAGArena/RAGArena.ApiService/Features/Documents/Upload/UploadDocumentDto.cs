using RAGArena.ApiService.Domain.Enums;

namespace RAGArena.ApiService.Features.Documents.Upload;

public class UploadDocumentRequest
{
    public IFormFile File { get; set; } = null!;
    public DocumentParserType Parser { get; set; } = DocumentParserType.LlamaParse;
    public ChunkingStrategy ChunkingStrategy { get; set; } = ChunkingStrategy.Semantic;
    public EmbeddingModelType EmbeddingModel { get; set; } = EmbeddingModelType.TextEmbedding3Small;
}

public record UploadDocumentResponse(
    Guid DocumentId,
    string FileName,
    long FileSizeBytes,
    int ChunkCount,
    string Status,
    long ProcessingTimeMs,
    string Parser,
    string ChunkingStrategy,
    string EmbeddingModel);
