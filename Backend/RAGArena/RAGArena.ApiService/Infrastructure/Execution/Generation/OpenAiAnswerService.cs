using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RAGArena.ApiService.Infrastructure.Execution.Generation;

public class OpenAiAnswerService(
    IHttpClientFactory httpClientFactory,
    IOptions<ExperimentExecutionOptions> options,
    ILogger<OpenAiAnswerService> logger) : ILlmAnswerService
{
    private readonly ExperimentExecutionOptions _options = options.Value;

    public async Task<LlmAnswerResult> GenerateAnswerAsync(string prompt, string caseLabel, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            logger.LogWarning("OpenAI API key not configured. Returning fallback answer for case {CaseLabel}", caseLabel);
            return new LlmAnswerResult(
                Answer: "OpenAI API key is not configured. This fallback response echoes the retrieved context path without external generation.",
                Model: "fallback-local",
                IsFallback: true);
        }

        var client = httpClientFactory.CreateClient("OpenAIChat");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var payload = new
        {
            model = _options.ChatModel,
            messages = new[]
            {
                new { role = "system", content = "You are a grounded RAG answerer. Use only the supplied context." },
                new { role = "user", content = prompt }
            },
            temperature = 0.2
        };

        using var request = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var response = await client.PostAsync("chat/completions", request, ct);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(ct);
        var chatResponse = JsonSerializer.Deserialize<OpenAiChatCompletionResponse>(content)
            ?? throw new InvalidOperationException("OpenAI chat completion returned null response.");

        var answer = chatResponse.Choices.FirstOrDefault()?.Message?.Content?.Trim();
        if (string.IsNullOrWhiteSpace(answer))
        {
            throw new InvalidOperationException("OpenAI chat completion returned an empty answer.");
        }

        return new LlmAnswerResult(
            Answer: answer,
            Model: chatResponse.Model ?? _options.ChatModel,
            IsFallback: false);
    }

    private sealed class OpenAiChatCompletionResponse
    {
        public string? Model { get; init; }
        public List<OpenAiChatChoice> Choices { get; init; } = [];
    }

    private sealed class OpenAiChatChoice
    {
        public OpenAiChatMessage? Message { get; init; }
    }

    private sealed class OpenAiChatMessage
    {
        public string? Content { get; init; }
    }
}
