using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace RAGArena.ApiService.Infrastructure.Parsers.LlamaParse;

public class LlamaParseService(
    IHttpClientFactory httpClientFactory,
    IOptions<LlamaParseOptions> options,
    ILogger<LlamaParseService> logger) : IDocumentParserService
{
    private readonly LlamaParseOptions _options = options.Value;

    public async Task<string> ParseAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            logger.LogWarning("LlamaParse API key not configured - using local text fallback when possible");
            return await FallbackReadAsync(fileStream, fileName, ct);
        }

        var client = httpClientFactory.CreateClient("LlamaParse");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var jobId = await UploadFileAsync(client, fileStream, fileName, ct);
        await PollUntilCompletedAsync(client, jobId, ct);
        return await FetchMarkdownResultAsync(client, jobId, ct);
    }

    private async Task<string> UploadFileAsync(HttpClient client, Stream fileStream, string fileName, CancellationToken ct)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", fileName);
        content.Add(new StringContent(_options.Language), "language");
        content.Add(new StringContent(_options.ResultType), "result_type");

        var response = await client.PostAsync("/api/parsing/upload", content, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LlamaParseUploadResponse>(ct)
            ?? throw new InvalidOperationException("LlamaParse upload returned null response");

        logger.LogInformation("LlamaParse job started: {JobId}", result.Id);
        return result.Id;
    }

    private async Task PollUntilCompletedAsync(HttpClient client, string jobId, CancellationToken ct)
    {
        for (var attempt = 0; attempt < _options.MaxPollingAttempts; attempt++)
        {
            await Task.Delay(_options.PollingIntervalMs, ct);

            var response = await client.GetFromJsonAsync<LlamaParseJobStatusResponse>(
                $"/api/parsing/job/{jobId}", ct)
                ?? throw new InvalidOperationException("LlamaParse job status returned null");

            logger.LogDebug("LlamaParse job {JobId} status: {Status} (attempt {Attempt})", jobId, response.Status, attempt + 1);

            if (response.Status == "SUCCESS")
            {
                return;
            }

            if (response.Status == "ERROR")
            {
                throw new InvalidOperationException($"LlamaParse job failed: {response.Error}");
            }
        }

        throw new TimeoutException($"LlamaParse job {jobId} did not complete within the polling limit");
    }

    private async Task<string> FetchMarkdownResultAsync(HttpClient client, string jobId, CancellationToken ct)
    {
        var response = await client.GetFromJsonAsync<LlamaParseMarkdownResponse>(
            $"/api/parsing/job/{jobId}/result/markdown", ct)
            ?? throw new InvalidOperationException("LlamaParse result returned null");

        logger.LogInformation("LlamaParse job {JobId} completed - {Length} chars", jobId, response.Markdown.Length);
        return response.Markdown;
    }

    private static async Task<string> FallbackReadAsync(Stream fileStream, string fileName, CancellationToken ct)
    {
        var extension = Path.GetExtension(fileName);
        var supportedFallbackExtensions = new[] { ".txt", ".md", ".csv", ".json" };

        if (!supportedFallbackExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "LlamaParse API key is required to parse PDF, DOC, and DOCX files in phase 2. Upload a text-based file or configure LlamaParse.");
        }

        using var reader = new StreamReader(fileStream, leaveOpen: true);
        return await reader.ReadToEndAsync(ct);
    }
}
