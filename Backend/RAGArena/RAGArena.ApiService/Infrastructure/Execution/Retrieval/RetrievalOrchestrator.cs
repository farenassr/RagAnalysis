using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RAGArena.ApiService.Domain.Enums;
using RAGArena.ApiService.Infrastructure.Embeddings;
using RAGArena.ApiService.Infrastructure.Persistence;

namespace RAGArena.ApiService.Infrastructure.Execution.Retrieval;

public class RetrievalOrchestrator(
    RAGArenaDbContext dbContext,
    IEmbeddingService embeddingService,
    IOptions<ExperimentExecutionOptions> options,
    ILogger<RetrievalOrchestrator> logger) : IRetrievalOrchestrator
{
    private readonly ExperimentExecutionOptions _options = options.Value;

    public async Task<IReadOnlyList<RetrievedChunkResult>> RetrieveAsync(
        string retrievalTechnique,
        string userQuestion,
        IReadOnlyList<Guid> documentIds,
        CancellationToken ct)
    {
        var chunks = await dbContext.DocumentChunks
            .AsNoTracking()
            .Include(chunk => chunk.Document)
            .Where(chunk => documentIds.Contains(chunk.DocumentId))
            .ToListAsync(ct);

        if (chunks.Count == 0)
        {
            return [];
        }

        var queryEmbedding = await embeddingService.GetEmbeddingAsync(
            userQuestion,
            EmbeddingModelType.TextEmbedding3Small,
            ct);

        var denseResults = chunks
            .Select(chunk => new RetrievedChunkResult(
                ChunkId: chunk.Id,
                DocumentId: chunk.DocumentId,
                DocumentFileName: chunk.Document.FileName,
                ChunkIndex: chunk.ChunkIndex,
                Content: chunk.Content,
                Score: CosineSimilarity(queryEmbedding, chunk.Embedding.ToArray()),
                RetrievalSource: "DenseVector"))
            .OrderByDescending(item => item.Score)
            .Take(_options.RetrievedChunkCount)
            .ToList();

        if (!string.Equals(retrievalTechnique, "Hybrid", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Executed dense vector retrieval with {Count} chunks", denseResults.Count);
            return denseResults;
        }

        var lexicalTerms = Tokenize(userQuestion);
        var hybridResults = chunks
            .Select(chunk =>
            {
                var lexicalScore = ComputeLexicalScore(lexicalTerms, chunk.Content);
                var denseScore = CosineSimilarity(queryEmbedding, chunk.Embedding.ToArray());
                var combinedScore = denseScore * _options.DenseRetrievalWeight + lexicalScore * _options.LexicalRetrievalWeight;

                return new RetrievedChunkResult(
                    ChunkId: chunk.Id,
                    DocumentId: chunk.DocumentId,
                    DocumentFileName: chunk.Document.FileName,
                    ChunkIndex: chunk.ChunkIndex,
                    Content: chunk.Content,
                    Score: combinedScore,
                    RetrievalSource: "Hybrid");
            })
            .OrderByDescending(item => item.Score)
            .Take(_options.RetrievedChunkCount)
            .ToList();

        logger.LogInformation("Executed hybrid retrieval stub with {Count} chunks", hybridResults.Count);
        return hybridResults;
    }

    private static double ComputeLexicalScore(HashSet<string> queryTerms, string content)
    {
        if (queryTerms.Count == 0)
        {
            return 0;
        }

        var chunkTerms = Tokenize(content);
        var overlap = queryTerms.Intersect(chunkTerms).Count();
        return overlap / (double)queryTerms.Count;
    }

    private static HashSet<string> Tokenize(string value) =>
        value
            .Split([' ', '\n', '\r', '\t', '.', ',', ':', ';', '!', '?', '(', ')', '[', ']', '{', '}', '"', '\''], StringSplitOptions.RemoveEmptyEntries)
            .Select(term => term.Trim().ToLowerInvariant())
            .Where(term => term.Length > 2)
            .ToHashSet();

    private static double CosineSimilarity(float[] left, float[] right)
    {
        double dot = 0;
        double leftNorm = 0;
        double rightNorm = 0;

        for (var index = 0; index < Math.Min(left.Length, right.Length); index++)
        {
            dot += left[index] * right[index];
            leftNorm += left[index] * left[index];
            rightNorm += right[index] * right[index];
        }

        var denominator = Math.Sqrt(leftNorm) * Math.Sqrt(rightNorm);
        return denominator == 0 ? 0 : dot / denominator;
    }
}
