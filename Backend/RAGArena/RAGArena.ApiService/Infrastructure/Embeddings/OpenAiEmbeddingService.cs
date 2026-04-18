using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;
using RAGArena.ApiService.Domain.Enums;

namespace RAGArena.ApiService.Infrastructure.Embeddings;

public class OpenAiEmbeddingService(
    IOptions<EmbeddingOptions> options,
    ILogger<OpenAiEmbeddingService> logger) : IEmbeddingService
{
    private readonly EmbeddingOptions _options = options.Value;

    public async Task<float[]> GetEmbeddingAsync(string text, EmbeddingModelType modelType, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            logger.LogWarning("OpenAI API key not configured - returning zero vector");
            return new float[GetVectorSize(modelType)];
        }

        var client = CreateClient(modelType);
        var result = await client.GenerateEmbeddingAsync(text, cancellationToken: ct);
        return result.Value.ToFloats().ToArray();
    }

    public async Task<IReadOnlyList<float[]>> GetEmbeddingsBatchAsync(
        IEnumerable<string> texts,
        EmbeddingModelType modelType,
        CancellationToken ct = default)
    {
        var textList = texts.ToList();

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            logger.LogWarning("OpenAI API key not configured - returning zero vectors");
            return textList.Select(_ => new float[GetVectorSize(modelType)]).ToList();
        }

        var client = CreateClient(modelType);
        var allEmbeddings = new List<float[]>();

        foreach (var batch in textList.Chunk(_options.BatchSize))
        {
            var result = await client.GenerateEmbeddingsAsync(batch, cancellationToken: ct);
            allEmbeddings.AddRange(result.Value.Select(embedding => embedding.ToFloats().ToArray()));
        }

        return allEmbeddings;
    }

    private EmbeddingClient CreateClient(EmbeddingModelType modelType)
    {
        var model = ResolveModelName(modelType);
        var openAiClient = new OpenAIClient(_options.ApiKey);
        return openAiClient.GetEmbeddingClient(model);
    }

    private string ResolveModelName(EmbeddingModelType modelType) =>
        modelType switch
        {
            EmbeddingModelType.TextEmbedding3Small => "text-embedding-3-small",
            EmbeddingModelType.TextEmbedding3Large => "text-embedding-3-large",
            EmbeddingModelType.AzureOpenAi => throw new NotSupportedException(
                "Azure OpenAI embeddings are not implemented in phase 2."),
            _ => _options.DefaultModel
        };

    private static int GetVectorSize(EmbeddingModelType modelType) =>
        modelType switch
        {
            EmbeddingModelType.TextEmbedding3Large => 3072,
            _ => 1536
        };
}
