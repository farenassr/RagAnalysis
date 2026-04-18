namespace RAGArena.ApiService.Infrastructure.Execution.Retrieval;

public interface IRetrievalOrchestrator
{
    Task<IReadOnlyList<RetrievedChunkResult>> RetrieveAsync(
        string retrievalTechnique,
        string userQuestion,
        IReadOnlyList<Guid> documentIds,
        CancellationToken ct);
}

public record RetrievedChunkResult(
    Guid ChunkId,
    Guid DocumentId,
    string DocumentFileName,
    int ChunkIndex,
    string Content,
    double Score,
    string RetrievalSource);
