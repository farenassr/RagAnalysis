using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using RAGArena.ApiService.Domain.Enums;
using RAGArena.ApiService.Infrastructure.Execution;
using RAGArena.ApiService.Infrastructure.Execution.Generation;
using RAGArena.ApiService.Infrastructure.Execution.Prompting;
using RAGArena.ApiService.Infrastructure.Execution.Retrieval;
using RAGArena.ApiService.Infrastructure.Execution.Tracing;
using RAGArena.ApiService.Infrastructure.Persistence;

namespace RAGArena.ApiService.Features.Experiments.ExecuteBatch;

public class ExecuteBatchHandler(
    RAGArenaDbContext dbContext,
    IRetrievalOrchestrator retrievalOrchestrator,
    IPromptTemplateService promptTemplateService,
    ILlmAnswerService llmAnswerService,
    ILangfuseTracingService langfuseTracingService,
    ILogger<ExecuteBatchHandler> logger)
{
    public async Task<ExecuteBatchResponse> HandleAsync(ExecuteBatchRequest request, CancellationToken ct)
    {
        ValidateRequest(request);

        var availableDocumentIds = await dbContext.Documents
            .AsNoTracking()
            .Where(document =>
                request.SharedDocumentIds.Contains(document.Id) &&
                document.Status == DocumentProcessingStatus.Completed)
            .Select(document => document.Id)
            .ToListAsync(ct);

        if (availableDocumentIds.Count != request.SharedDocumentIds.Count)
        {
            throw new InvalidOperationException("One or more selected documents are unavailable for execution.");
        }

        var batchId = Guid.NewGuid().ToString("N");
        var results = new List<ExecuteExperimentCaseResultDto>();

        using var batchActivity = ExecutionActivitySource.Instance.StartActivity("experiments.execute-batch");
        batchActivity?.SetTag("rag.batch.id", batchId);
        batchActivity?.SetTag("rag.case.count", request.Cases.Count);

        foreach (var experimentCase in request.Cases)
        {
            using var caseActivity = ExecutionActivitySource.Instance.StartActivity("experiments.execute-case");
            caseActivity?.SetTag("rag.batch.id", batchId);
            caseActivity?.SetTag("rag.case.label", experimentCase.CaseLabel);
            caseActivity?.SetTag("rag.retrieval.technique", experimentCase.RetrievalTechnique);

            var totalStopwatch = Stopwatch.StartNew();
            var retrievalStopwatch = Stopwatch.StartNew();

            var retrievedChunks = await retrievalOrchestrator.RetrieveAsync(
                experimentCase.RetrievalTechnique,
                request.QueryText,
                request.SharedDocumentIds,
                ct);

            retrievalStopwatch.Stop();

            var prompt = promptTemplateService.BuildPrompt(
                experimentCase.PromptTemplate,
                request.QueryText,
                retrievedChunks.Select(chunk => chunk.Content).ToList());

            var generationStopwatch = Stopwatch.StartNew();
            var answerResult = await llmAnswerService.GenerateAnswerAsync(prompt, experimentCase.CaseLabel, ct);
            generationStopwatch.Stop();
            totalStopwatch.Stop();

            var warnings = BuildWarnings(experimentCase, answerResult);

            await langfuseTracingService.RecordExecutionAsync(
                batchId,
                experimentCase.CaseLabel,
                request.QueryText,
                retrievedChunks.Select(chunk => chunk.ChunkId.ToString()).ToList(),
                totalStopwatch.ElapsedMilliseconds,
                ct);

            results.Add(new ExecuteExperimentCaseResultDto(
                CaseLabel: experimentCase.CaseLabel,
                RetrievalTechnique: experimentCase.RetrievalTechnique,
                Answer: answerResult.Answer,
                Model: answerResult.Model,
                TotalLatencyMs: totalStopwatch.ElapsedMilliseconds,
                RetrievalLatencyMs: retrievalStopwatch.ElapsedMilliseconds,
                GenerationLatencyMs: generationStopwatch.ElapsedMilliseconds,
                UsedFallbackAnswer: answerResult.IsFallback,
                RetrievedChunks: retrievedChunks.Select(chunk => new RetrievedChunkDto(
                    ChunkId: chunk.ChunkId,
                    DocumentId: chunk.DocumentId,
                    DocumentFileName: chunk.DocumentFileName,
                    ChunkIndex: chunk.ChunkIndex,
                    Score: Math.Round(chunk.Score, 4),
                    RetrievalSource: chunk.RetrievalSource,
                    ContentPreview: BuildContentPreview(chunk.Content))).ToList(),
                Warnings: warnings));
        }

        logger.LogInformation("Executed batch {BatchId} for {CaseCount} cases", batchId, results.Count);

        return new ExecuteBatchResponse(
            BatchId: batchId,
            QueryText: request.QueryText,
            CaseCount: results.Count,
            Results: results);
    }

    private static void ValidateRequest(ExecuteBatchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.QueryText))
        {
            throw new InvalidOperationException("A query is required before executing the batch.");
        }

        if (request.SharedDocumentIds.Count == 0)
        {
            throw new InvalidOperationException("At least one processed document is required.");
        }

        if (request.Cases.Count == 0)
        {
            throw new InvalidOperationException("At least one experiment case is required.");
        }

        foreach (var experimentCase in request.Cases)
        {
            if (!string.Equals(experimentCase.Parser, "LlamaParse", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("Phase 4 execution supports LlamaParse-indexed documents only.");
            }

            if (!string.Equals(experimentCase.EmbeddingModel, "TextEmbedding3Small", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("Phase 4 execution supports text-embedding-3-small only.");
            }

            if (!string.Equals(experimentCase.RetrievalTechnique, "DenseVector", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(experimentCase.RetrievalTechnique, "Hybrid", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("Phase 4 execution supports DenseVector and Hybrid retrieval only.");
            }
        }
    }

    private static List<string> BuildWarnings(ExecuteExperimentCaseDto experimentCase, LlmAnswerResult answerResult)
    {
        var warnings = new List<string>();

        if (!string.Equals(experimentCase.Reranker, "None", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("Selected reranker is not active yet in phase 4 execution.");
        }

        if (answerResult.IsFallback)
        {
            warnings.Add("Returned fallback answer because OpenAI chat completion is not configured.");
        }

        if (string.Equals(experimentCase.RetrievalTechnique, "Hybrid", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("Hybrid retrieval is currently a weighted dense-plus-lexical stub.");
        }

        return warnings;
    }

    private static string BuildContentPreview(string content)
    {
        const int maxLength = 220;
        var normalized = content.Replace("\r", " ").Replace("\n", " ").Trim();
        return normalized.Length <= maxLength ? normalized : $"{normalized[..maxLength]}...";
    }
}
