using FastEndpoints;

namespace RAGArena.ApiService.Features.Documents.Upload;

public class UploadDocumentEndpoint(UploadDocumentHandler handler) : Endpoint<UploadDocumentRequest, UploadDocumentResponse>
{
    public override void Configure()
    {
        Post("/api/documents/upload");
        AllowAnonymous();
        AllowFileUploads();
        Summary(s =>
        {
            s.Summary = "Upload and process a document";
            s.Description = "Parses, chunks, embeds, and stores a document using the selected pipeline configuration";
        });
    }

    public override async Task HandleAsync(UploadDocumentRequest req, CancellationToken ct)
    {
        var response = await handler.HandleAsync(req, ct);
        await Send.OkAsync(response, ct);
    }
}
