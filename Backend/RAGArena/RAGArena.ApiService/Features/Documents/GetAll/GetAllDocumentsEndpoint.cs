using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using RAGArena.ApiService.Infrastructure.Persistence;

namespace RAGArena.ApiService.Features.Documents.GetAll;

public class GetAllDocumentsEndpoint(RAGArenaDbContext dbContext) : Endpoint<EmptyRequest, GetAllDocumentsResponse>
{
    public override void Configure()
    {
        Get("/api/documents");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "List all documents";
            s.Description = "Returns all processed documents with their metadata and processing results";
        });
    }

    public override async Task HandleAsync(EmptyRequest _, CancellationToken ct)
    {
        var documents = await dbContext.Documents
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DocumentSummaryDto(
                d.Id,
                d.FileName,
                d.FileSizeBytes,
                d.ChunkCount,
                d.Status.ToString(),
                d.ParserType.ToString(),
                d.ChunkingStrategy.ToString(),
                d.EmbeddingModel.ToString(),
                d.ProcessingTimeMs,
                d.CreatedAt,
                d.ProcessedAt))
            .ToListAsync(ct);

        await Send.OkAsync(new GetAllDocumentsResponse(documents, documents.Count), ct);
    }
}
