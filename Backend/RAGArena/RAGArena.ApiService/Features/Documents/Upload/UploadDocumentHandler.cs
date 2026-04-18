using RAGArena.ApiService.Domain.Entities;
using RAGArena.ApiService.Domain.Enums;
using RAGArena.ApiService.Infrastructure.Chunking;
using RAGArena.ApiService.Infrastructure.Embeddings;
using RAGArena.ApiService.Infrastructure.Parsers;
using RAGArena.ApiService.Infrastructure.Persistence;
using RAGArena.ApiService.Infrastructure.Storage;
using System.Diagnostics;
using Pgvector;

namespace RAGArena.ApiService.Features.Documents.Upload;

public class UploadDocumentHandler(
    RAGArenaDbContext dbContext,
    IDocumentStorageService storageService,
    IServiceProvider serviceProvider,
    IEmbeddingService embeddingService,
    ILogger<UploadDocumentHandler> logger)
{
    public async Task<UploadDocumentResponse> HandleAsync(UploadDocumentRequest request, CancellationToken ct)
    {
        ValidatePhaseTwoSupport(request);

        var stopwatch = Stopwatch.StartNew();

        var document = new Document
        {
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSizeBytes = request.File.Length,
            ParserType = request.Parser,
            ChunkingStrategy = request.ChunkingStrategy,
            EmbeddingModel = request.EmbeddingModel,
            Status = DocumentProcessingStatus.Processing
        };

        dbContext.Documents.Add(document);
        await dbContext.SaveChangesAsync(ct);

        try
        {
            await using var fileStream = request.File.OpenReadStream();
            document.StoragePath = await storageService.SaveAsync(fileStream, request.File.FileName, ct);

            var parsedText = await ParseDocumentAsync(request, document.StoragePath, ct);
            var chunks = await ChunkTextAsync(request.ChunkingStrategy, parsedText, ct);
            var documentChunks = await EmbedAndBuildChunksAsync(document.Id, chunks, request.EmbeddingModel, ct);

            dbContext.DocumentChunks.AddRange(documentChunks);
            document.ChunkCount = documentChunks.Count;
            document.Status = DocumentProcessingStatus.Completed;
            document.ProcessedAt = DateTime.UtcNow;
            document.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;

            await dbContext.SaveChangesAsync(ct);

            logger.LogInformation("Document {Id} processed: {Chunks} chunks in {Ms}ms",
                document.Id, document.ChunkCount, document.ProcessingTimeMs);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process document {Id}", document.Id);
            document.Status = DocumentProcessingStatus.Failed;
            document.ErrorMessage = ex.Message;
            document.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
            await dbContext.SaveChangesAsync(ct);
            throw;
        }

        return new UploadDocumentResponse(
            DocumentId: document.Id,
            FileName: document.FileName,
            FileSizeBytes: document.FileSizeBytes,
            ChunkCount: document.ChunkCount,
            Status: document.Status.ToString(),
            ProcessingTimeMs: document.ProcessingTimeMs,
            Parser: document.ParserType.ToString(),
            ChunkingStrategy: document.ChunkingStrategy.ToString(),
            EmbeddingModel: document.EmbeddingModel.ToString());
    }

    private async Task<string> ParseDocumentAsync(UploadDocumentRequest request, string storagePath, CancellationToken ct)
    {
        var parserKey = request.Parser.ToString();
        var parser = serviceProvider.GetRequiredKeyedService<IDocumentParserService>(parserKey);

        await using var stream = File.OpenRead(storagePath);
        return await parser.ParseAsync(stream, request.File.FileName, ct);
    }

    private async Task<IReadOnlyList<string>> ChunkTextAsync(ChunkingStrategy strategy, string text, CancellationToken ct)
    {
        var chunker = serviceProvider.GetRequiredKeyedService<IChunkingService>(strategy.ToString());
        return await chunker.ChunkAsync(text, ct);
    }

    private async Task<List<DocumentChunk>> EmbedAndBuildChunksAsync(
        Guid documentId,
        IReadOnlyList<string> chunks,
        EmbeddingModelType modelType,
        CancellationToken ct)
    {
        var embeddings = await embeddingService.GetEmbeddingsBatchAsync(chunks, modelType, ct);

        return chunks.Select((content, index) => new DocumentChunk
        {
            DocumentId = documentId,
            ChunkIndex = index,
            Content = content,
            Embedding = new Vector(embeddings[index]),
            TokenCount = EstimateTokenCount(content)
        }).ToList();
    }

    private static int EstimateTokenCount(string text) =>
        (int)Math.Ceiling(text.Length / 4.0);

    private static void ValidatePhaseTwoSupport(UploadDocumentRequest request)
    {
        if (request.Parser is not DocumentParserType.LlamaParse)
        {
            throw new NotSupportedException("Phase 2 supports LlamaParse only.");
        }

        if (request.EmbeddingModel is not EmbeddingModelType.TextEmbedding3Small)
        {
            throw new NotSupportedException("Phase 2 supports text-embedding-3-small only.");
        }
    }
}
