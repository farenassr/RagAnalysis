namespace RAGArena.ApiService.Configuration;

public class LangfuseOptions
{
    public const string SectionName = "Langfuse";

    public string PublicKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://cloud.langfuse.com";
    public bool Enabled { get; init; } = false;
}
