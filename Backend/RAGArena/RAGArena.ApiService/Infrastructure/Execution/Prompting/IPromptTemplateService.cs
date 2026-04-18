namespace RAGArena.ApiService.Infrastructure.Execution.Prompting;

public interface IPromptTemplateService
{
    string BuildPrompt(string templateName, string userQuestion, IReadOnlyList<string> contextChunks);
}
