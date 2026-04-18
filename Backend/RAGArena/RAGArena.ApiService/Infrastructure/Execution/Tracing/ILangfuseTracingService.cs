namespace RAGArena.ApiService.Infrastructure.Execution.Tracing;

public interface ILangfuseTracingService
{
    Task RecordExecutionAsync(
        string batchId,
        string caseLabel,
        string queryText,
        IReadOnlyList<string> retrievedChunkIds,
        long latencyMs,
        CancellationToken ct);
}
