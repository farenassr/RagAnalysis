using FastEndpoints;

namespace RAGArena.ApiService.Features.ExperimentBuilder.GetOptions;

public class GetExperimentBuilderOptionsEndpoint : EndpointWithoutRequest<GetExperimentBuilderOptionsResponse>
{
    public override void Configure()
    {
        Get("/api/experiment-builder/options");
        AllowAnonymous();
        Summary(summary =>
        {
            summary.Summary = "List experiment builder configuration options";
            summary.Description = "Returns the available parser, chunking, retrieval, embedding, reranker, prompt, evaluation, and database options for phase 3.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new GetExperimentBuilderOptionsResponse(
            Parsers:
            [
                new("LlamaParse", "LlamaParse", "Cloud-based parser for PDFs and complex document layouts.", true, true),
                new("AzureDocumentIntelligence", "Azure Document Intelligence", "Azure-native document extraction for structured and scanned files.", false)
            ],
            ChunkingStrategies:
            [
                new("Semantic", "Semantic Chunking", "Splits content at semantic shifts using embedding similarity.", true, true),
                new("DocumentAware", "Document-Aware Chunking", "Preserves section and layout structure while chunking.", true),
                new("RecursiveCharacter", "Recursive Character Chunking", "Produces fallback chunks with recursive text splitting.", true),
                new("StructureAware", "Structure-Aware Chunking", "Anchors chunks to headings, lists, and structural cues.", true)
            ],
            RetrievalTechniques:
            [
                new("DenseVector", "Dense Vector Retrieval", "Nearest-neighbor retrieval over pgvector-backed embeddings.", true, true),
                new("Bm25", "BM25", "Keyword-first lexical retrieval for sparse matching.", false),
                new("Hybrid", "Hybrid Retrieval", "Combines dense and lexical retrieval in a single ranked set.", true),
                new("HybridRrf", "Hybrid Retrieval + RRF", "Fuses multiple ranked result lists using reciprocal rank fusion.", false),
                new("AzureAiSearch", "Azure AI Search", "Managed retrieval using Azure AI Search indexes.", false),
                new("QueryRewriting", "Query Rewriting", "Rephrases the query before retrieval.", false),
                new("QueryExpansion", "Query Expansion", "Expands the query with related terms or sub-queries.", false),
                new("HyDE", "HyDE", "Generates a hypothetical answer to improve retrieval recall.", false),
                new("RerankingPipeline", "Re-ranking Pipeline", "Runs retrieval followed by ranking refinement.", false)
            ],
            EmbeddingModels:
            [
                new("TextEmbedding3Small", "text-embedding-3-small", "Default phase 2 indexing model with compact vectors.", true, true),
                new("TextEmbedding3Large", "text-embedding-3-large", "Higher-capacity embedding model for improved semantic separation.", false),
                new("AzureOpenAi", "Azure OpenAI Embeddings", "Azure-hosted embedding deployment.", false)
            ],
            Rerankers:
            [
                new("None", "No Reranker", "Skips post-retrieval reranking.", true, true),
                new("CohereRerank", "Cohere Rerank", "Applies Cohere reranking on retrieved candidates.", false),
                new("BgeRerankerAzure", "BGE-Reranker (Azure AI)", "Azure-hosted BGE reranker for semantic ordering.", false)
            ],
            PromptTemplates:
            [
                new("GroundedAnswer", "Grounded Answer", "Direct grounded answer with explicit context usage.", true, true),
                new("AnalystBrief", "Analyst Brief", "Structured summary for comparative enterprise analysis.", true),
                new("EvidenceFirst", "Evidence First", "Requires evidence extraction before final answer synthesis.", true)
            ],
            EvaluationFrameworks:
            [
                new("ArenaBaseline", "Arena Baseline", "Local validation schema for phase 3 payload verification.", true, true),
                new("Langfuse", "Langfuse", "Tracing and evaluation-ready external framework.", false),
                new("AzurePromptFlow", "Azure AI Prompt Flow", "Azure evaluation orchestration and scoring framework.", false)
            ],
            DatabaseTargets:
            [
                new("PostgreSqlPgVector", "PostgreSQL + pgvector", "Primary vector-capable store for the arena.", true, true)
            ]);

        await Send.OkAsync(response, ct);
    }
}
