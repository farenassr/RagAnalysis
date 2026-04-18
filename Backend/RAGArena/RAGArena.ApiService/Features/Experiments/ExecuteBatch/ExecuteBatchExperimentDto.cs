namespace RAGArena.ApiService.Features.Experiments.ExecuteBatch;

public class ExecuteBatchRequest
{
    public string QueryText { get; set; } = string.Empty;
    public List<Guid> SharedDocumentIds { get; set; } = [];
    public List<ExecuteExperimentCaseDto> Cases { get; set; } = [];
}

public class ExecuteExperimentCaseDto
{
    public string CaseLabel { get; set; } = string.Empty;
    public string Parser { get; set; } = string.Empty;
    public string ChunkingStrategy { get; set; } = string.Empty;
    public string RetrievalTechnique { get; set; } = string.Empty;
    public string EmbeddingModel { get; set; } = string.Empty;
    public string Reranker { get; set; } = string.Empty;
    public string PromptTemplate { get; set; } = string.Empty;
    public string EvaluationFramework { get; set; } = string.Empty;
    public string DatabaseTarget { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}

public record ExecuteBatchResponse(
    string BatchId,
    string QueryText,
    int CaseCount,
    IReadOnlyList<ExecuteExperimentCaseResultDto> Results);

public record ExecuteExperimentCaseResultDto(
    string CaseLabel,
    string RetrievalTechnique,
    string Answer,
    string Model,
    long TotalLatencyMs,
    long RetrievalLatencyMs,
    long GenerationLatencyMs,
    bool UsedFallbackAnswer,
    IReadOnlyList<RetrievedChunkDto> RetrievedChunks,
    IReadOnlyList<string> Warnings);

public record RetrievedChunkDto(
    Guid ChunkId,
    Guid DocumentId,
    string DocumentFileName,
    int ChunkIndex,
    double Score,
    string RetrievalSource,
    string ContentPreview);
