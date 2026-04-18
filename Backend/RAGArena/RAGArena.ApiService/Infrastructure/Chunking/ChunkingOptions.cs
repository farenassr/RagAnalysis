namespace RAGArena.ApiService.Infrastructure.Chunking;

public class ChunkingOptions
{
    public const string SectionName = "Chunking";

    public int MaxChunkSize { get; init; } = 512;
    public int ChunkOverlap { get; init; } = 50;
    public double SemanticBreakpointThreshold { get; init; } = 0.3;
    public int SemanticWindowSize { get; init; } = 3;
}
