using FastEndpoints;
using RAGArena.ApiService.Infrastructure.Persistence;

namespace RAGArena.ApiService.Features.Health;

public class GetApiStatusEndpoint(RAGArenaDbContext dbContext) : Endpoint<EmptyRequest, ApiStatusResponse>
{
    public override void Configure()
    {
        Get("/api/status");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get API status";
            s.Description = "Returns the current health status of the RAG Arena API and its dependencies";
        });
    }

    public override async Task HandleAsync(EmptyRequest _, CancellationToken ct)
    {
        var canConnectToDb = await dbContext.Database.CanConnectAsync(ct);

        await Send.OkAsync(new ApiStatusResponse(
            Status: canConnectToDb ? "Healthy" : "Degraded",
            Version: "1.0.0",
            DatabaseConnected: canConnectToDb,
            Timestamp: DateTime.UtcNow
        ), ct);
    }
}

public record ApiStatusResponse(
    string Status,
    string Version,
    bool DatabaseConnected,
    DateTime Timestamp);
