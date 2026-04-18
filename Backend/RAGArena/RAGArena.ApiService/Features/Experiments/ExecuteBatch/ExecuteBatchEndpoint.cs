using FastEndpoints;

namespace RAGArena.ApiService.Features.Experiments.ExecuteBatch;

public class ExecuteBatchEndpoint(ExecuteBatchHandler handler) : Endpoint<ExecuteBatchRequest, ExecuteBatchResponse>
{
    public override void Configure()
    {
        Post("/api/experiments/execute-batch");
        AllowAnonymous();
        Summary(summary =>
        {
            summary.Summary = "Execute a batch of RAG experiment cases";
            summary.Description = "Runs the selected experiment configurations against the same shared document set and returns retrieved context plus generated answers.";
        });
    }

    public override async Task HandleAsync(ExecuteBatchRequest req, CancellationToken ct)
    {
        var response = await handler.HandleAsync(req, ct);
        await Send.OkAsync(response, ct);
    }
}
