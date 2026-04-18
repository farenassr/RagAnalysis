using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace RAGArena.ApiService.Infrastructure.Chunking;

public class RecursiveCharacterChunkingService(
    IOptions<ChunkingOptions> options,
    ILogger<RecursiveCharacterChunkingService> logger) : IChunkingService
{
    private readonly ChunkingOptions _options = options.Value;

    private static readonly string[] Separators = ["\n\n", "\n", ". ", " ", ""];

    public Task<IReadOnlyList<string>> ChunkAsync(string text, CancellationToken ct = default)
    {
        var chunks = SplitRecursively(text.Trim(), Separators, 0);
        var merged = MergeWithOverlap(chunks);

        logger.LogInformation("Recursive chunking produced {Count} chunks", merged.Count);
        return Task.FromResult<IReadOnlyList<string>>(merged);
    }

    private List<string> SplitRecursively(string text, string[] separators, int depth)
    {
        if (text.Length <= _options.MaxChunkSize || depth >= separators.Length)
            return [text];

        var separator = separators[depth];
        var parts = string.IsNullOrEmpty(separator)
            ? SplitByLength(text)
            : text.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                  .Select(p => p.Trim())
                  .Where(p => !string.IsNullOrWhiteSpace(p))
                  .ToArray();

        var result = new List<string>();
        foreach (var part in parts)
        {
            if (part.Length <= _options.MaxChunkSize)
                result.Add(part);
            else
                result.AddRange(SplitRecursively(part, separators, depth + 1));
        }

        return result;
    }

    private string[] SplitByLength(string text)
    {
        var chunks = new List<string>();
        for (var i = 0; i < text.Length; i += _options.MaxChunkSize)
            chunks.Add(text[i..Math.Min(i + _options.MaxChunkSize, text.Length)]);
        return [.. chunks];
    }

    private List<string> MergeWithOverlap(List<string> chunks)
    {
        if (_options.ChunkOverlap == 0 || chunks.Count <= 1)
            return chunks;

        var merged = new List<string> { chunks[0] };

        for (var i = 1; i < chunks.Count; i++)
        {
            var overlap = GetOverlapText(merged[^1], _options.ChunkOverlap);
            merged.Add(overlap + chunks[i]);
        }

        return merged;
    }

    private static string GetOverlapText(string text, int overlapTokens)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var overlapWords = words.TakeLast(overlapTokens).ToArray();
        return overlapWords.Length > 0 ? string.Join(" ", overlapWords) + " " : string.Empty;
    }
}
