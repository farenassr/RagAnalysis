namespace RAGArena.ApiService.Infrastructure.Embeddings;

public class EmbeddingOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; init; } = string.Empty;
    public string DefaultModel { get; init; } = "text-embedding-3-small";
    public int BatchSize { get; init; } = 20;
}
