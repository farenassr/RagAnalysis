using System.Text.Json.Serialization;

namespace RAGArena.ApiService.Infrastructure.Parsers.LlamaParse;

public record LlamaParseUploadResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("status")] string Status);

public record LlamaParseJobStatusResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("error")] string? Error);

public record LlamaParseMarkdownResponse(
    [property: JsonPropertyName("markdown")] string Markdown);
