namespace RAGArena.ApiService.Infrastructure.Execution;

public class ExperimentExecutionOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://api.openai.com/v1";
    public string ChatModel { get; init; } = "gpt-4o-mini";
    public int RetrievedChunkCount { get; init; } = 5;
    public double DenseRetrievalWeight { get; init; } = 0.7;
    public double LexicalRetrievalWeight { get; init; } = 0.3;
}
