namespace RAGArena.ApiService.Infrastructure.Execution.Generation;

public interface ILlmAnswerService
{
    Task<LlmAnswerResult> GenerateAnswerAsync(string prompt, string caseLabel, CancellationToken ct);
}

public record LlmAnswerResult(
    string Answer,
    string Model,
    bool IsFallback);
