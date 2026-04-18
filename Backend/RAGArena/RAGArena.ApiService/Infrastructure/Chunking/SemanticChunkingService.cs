using Microsoft.Extensions.Options;
using RAGArena.ApiService.Domain.Enums;
using RAGArena.ApiService.Infrastructure.Embeddings;
using System.Text.RegularExpressions;

namespace RAGArena.ApiService.Infrastructure.Chunking;

public class SemanticChunkingService(
    IEmbeddingService embeddingService,
    IOptions<ChunkingOptions> options,
    ILogger<SemanticChunkingService> logger) : IChunkingService
{
    private readonly ChunkingOptions _options = options.Value;

    private static readonly Regex SentenceSplitter =
        new(@"(?<=[.!?])\s+", RegexOptions.Compiled);

    public async Task<IReadOnlyList<string>> ChunkAsync(string text, CancellationToken ct = default)
    {
        var sentences = SentenceSplitter.Split(text.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .ToList();

        if (sentences.Count <= _options.SemanticWindowSize)
            return [text.Trim()];

        var windows = BuildWindows(sentences);
        var windowEmbeddings = await embeddingService.GetEmbeddingsBatchAsync(
            windows,
            EmbeddingModelType.TextEmbedding3Small,
            ct);

        var distances = ComputeConsecutiveDistances(windowEmbeddings);
        var breakpoints = FindBreakpoints(distances);

        var chunks = BuildChunks(sentences, breakpoints);
        logger.LogInformation("Semantic chunking produced {Count} chunks from {Sentences} sentences", chunks.Count, sentences.Count);

        return chunks;
    }

    private List<string> BuildWindows(List<string> sentences)
    {
        var windows = new List<string>();
        var w = _options.SemanticWindowSize;

        for (var i = 0; i < sentences.Count; i++)
        {
            var start = Math.Max(0, i - w / 2);
            var end = Math.Min(sentences.Count, start + w);
            windows.Add(string.Join(" ", sentences[start..end]));
        }

        return windows;
    }

    private static List<double> ComputeConsecutiveDistances(IReadOnlyList<float[]> embeddings)
    {
        var distances = new List<double>();
        for (var i = 0; i < embeddings.Count - 1; i++)
            distances.Add(1.0 - CosineSimilarity(embeddings[i], embeddings[i + 1]));
        return distances;
    }

    private List<int> FindBreakpoints(List<double> distances)
    {
        if (distances.Count == 0) return [];

        var mean = distances.Average();
        var stdDev = Math.Sqrt(distances.Average(d => Math.Pow(d - mean, 2)));
        var threshold = mean + stdDev * _options.SemanticBreakpointThreshold;

        return distances
            .Select((d, i) => (Distance: d, Index: i))
            .Where(x => x.Distance > threshold)
            .Select(x => x.Index + 1)
            .ToList();
    }

    private static List<string> BuildChunks(List<string> sentences, List<int> breakpoints)
    {
        var chunks = new List<string>();
        var lastBreak = 0;

        foreach (var bp in breakpoints)
        {
            var chunk = string.Join(" ", sentences[lastBreak..bp]);
            if (!string.IsNullOrWhiteSpace(chunk))
                chunks.Add(chunk);
            lastBreak = bp;
        }

        var remaining = string.Join(" ", sentences[lastBreak..]);
        if (!string.IsNullOrWhiteSpace(remaining))
            chunks.Add(remaining);

        return chunks;
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        float dot = 0, normA = 0, normB = 0;
        for (var i = 0; i < Math.Min(a.Length, b.Length); i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        var denom = MathF.Sqrt(normA) * MathF.Sqrt(normB);
        return denom == 0 ? 0 : dot / denom;
    }
}
