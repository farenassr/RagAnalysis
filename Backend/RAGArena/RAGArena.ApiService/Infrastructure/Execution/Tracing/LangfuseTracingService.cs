using Microsoft.Extensions.Options;
using RAGArena.ApiService.Configuration;

namespace RAGArena.ApiService.Infrastructure.Execution.Tracing;

public class LangfuseTracingService(
    IOptions<LangfuseOptions> options,
    ILogger<LangfuseTracingService> logger) : ILangfuseTracingService
{
    private readonly LangfuseOptions _options = options.Value;

    public Task RecordExecutionAsync(
        string batchId,
        string caseLabel,
        string queryText,
        IReadOnlyList<string> retrievedChunkIds,
        long latencyMs,
        CancellationToken ct)
    {
        if (!_options.Enabled)
        {
            return Task.CompletedTask;
        }

        logger.LogInformation(
            "Langfuse tracing enabled for batch {BatchId}, case {CaseLabel}, query length {QueryLength}, retrieved chunks {ChunkCount}, latency {LatencyMs}ms",
            batchId,
            caseLabel,
            queryText.Length,
            retrievedChunkIds.Count,
            latencyMs);

        return Task.CompletedTask;
    }
}
