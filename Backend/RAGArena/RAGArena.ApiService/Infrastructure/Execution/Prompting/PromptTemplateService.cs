namespace RAGArena.ApiService.Infrastructure.Execution.Prompting;

public class PromptTemplateService : IPromptTemplateService
{
    public string BuildPrompt(string templateName, string userQuestion, IReadOnlyList<string> contextChunks)
    {
        var contextBlock = string.Join(
            "\n\n---\n\n",
            contextChunks.Select((chunk, index) => $"Chunk {index + 1}:\n{chunk}"));

        var instruction = templateName switch
        {
            "AnalystBrief" => "Write a concise analyst brief grounded only in the supplied context.",
            "EvidenceFirst" => "List the strongest evidence first, then answer the question using only the supplied context.",
            _ => "Answer the question using only the supplied context. If the answer is unsupported, say that the context is insufficient."
        };

        return
            $"""
            {instruction}

            User question:
            {userQuestion}

            Retrieved context:
            {contextBlock}
            """;
    }
}
