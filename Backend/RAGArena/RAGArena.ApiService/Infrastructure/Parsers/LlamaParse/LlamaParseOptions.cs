namespace RAGArena.ApiService.Infrastructure.Parsers.LlamaParse;

public class LlamaParseOptions
{
    public const string SectionName = "LlamaParse";

    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://api.cloud.llamaindex.ai";
    public int PollingIntervalMs { get; init; } = 2000;
    public int MaxPollingAttempts { get; init; } = 60;
    public string Language { get; init; } = "en";
    public string ResultType { get; init; } = "markdown";
}
