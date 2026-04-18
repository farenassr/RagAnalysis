namespace RAGArena.ApiService.Features.ExperimentBuilder.GetOptions;

public record BuilderOptionDto(
    string Value,
    string Label,
    string Description,
    bool IsAvailable,
    bool IsRecommended = false);

public record GetExperimentBuilderOptionsResponse(
    IReadOnlyList<BuilderOptionDto> Parsers,
    IReadOnlyList<BuilderOptionDto> ChunkingStrategies,
    IReadOnlyList<BuilderOptionDto> RetrievalTechniques,
    IReadOnlyList<BuilderOptionDto> EmbeddingModels,
    IReadOnlyList<BuilderOptionDto> Rerankers,
    IReadOnlyList<BuilderOptionDto> PromptTemplates,
    IReadOnlyList<BuilderOptionDto> EvaluationFrameworks,
    IReadOnlyList<BuilderOptionDto> DatabaseTargets);
