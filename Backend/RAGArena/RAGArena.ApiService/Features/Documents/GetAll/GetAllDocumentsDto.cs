namespace RAGArena.ApiService.Features.Documents.GetAll;

public record DocumentSummaryDto(
    Guid Id,
    string FileName,
    long FileSizeBytes,
    int ChunkCount,
    string Status,
    string Parser,
    string ChunkingStrategy,
    string EmbeddingModel,
    long ProcessingTimeMs,
    DateTime CreatedAt,
    DateTime? ProcessedAt);

public record GetAllDocumentsResponse(IReadOnlyList<DocumentSummaryDto> Documents, int Total);
