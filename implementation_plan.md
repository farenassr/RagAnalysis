# RAG Evaluation Arena - Implementation Plan

This plan breaks down the development of the **RAG Evaluation Arena** enterprise platform into testable, iterative steps as requested in the architecture rules. We will not proceed to the next step until the current step is thoroughly tested and verified.

## User Review Required

> [!IMPORTANT]
> Please review these steps carefully. Once you approve, we will begin executing **Phase 1**. Each phase acts as a checkpoint where we verify the functionality before continuing.

## Phase 1: Base Solution Structure (.NET Aspire, API, Frontend)

Our first step is to establish the scaffolding, orchestration, and core dependencies.

- **Backend**: Configure .NET Aspire Host, ServiceDefaults, and the .NET 10 API project.
- **Frontend**: Scaffold the React + TypeScript app using Vite.
- **Database**: Add a PostgreSQL container (with `pgvector`) to the Aspire orchestration and verify connection from the API using EF Core.
- **Observability**: Interconnect OpenTelemetry between the systems and lay the foundation for Langfuse keys/configuration.
- **API Foundation**: Setup FastEndpoints, vertical slice folder structure, and global error handling middleware.
- **Verification**: Run `dotnet run` on the AppHost. We will verify that the dashboard launches, PostgreSQL is seeded/running, the Swagger API is accessible, and the React frontend loads.

## Phase 2: Core Vertical Slice 1 - Ingestion & Chunking

Once the base is stable, we integrate file ingestion and storage.

- **Entity Model**: Create `Document` and `DocumentChunk` records using EF Core and pgvector `Vector` types.
- **API**: Implement the `Documents/Upload` vertical slice.
- **Parsers Integration**: Stub/Implement **LlamaParse** integration.
- **Chunking Integration**: Implement Semantic Chunking.
- **Embedding**: Connect to OpenAI `text-embedding-3-small` for initial indexing.
- **Frontend**: Build the shared file upload UI component (React Hook Form + TanStack Query fetching).
- **Verification**: The user uploads a document via the UI. We verify the chunks and embeddings are correctly stored in the PostgreSQL database.

## Phase 3: Multi-Case Configuration Builder (UI & State)

Building the core experiment setup workspace.

- **API**: Endpoints to list available tools (parsers, chunkers, retrieval methods, embedding models, rerankers).
- **Frontend**: Create the single-workspace configuration builder.
- **Frontend State**: Use Zustand to manage the addition, removal, and editing of multiple comparison cases.
- **Verification**: The user can dynamically add 3 distinct "Case" rows, configure them differently, and clicking "Run" correctly logs a validated batch configuration payload.

## Phase 4: Execution Engine & External Service Orchestration

Developing the logic to run the selected pipeline configurations.

- **API**: Implement the `Experiments/ExecuteBatch` vertical slice.
- **Retrievers**: Implement Dense Vector Retrieval (pgvector) and a basic Hybrid Retrieval stub.
- **LLM/Reranking Integration**: Inject Azure OpenAI / ChatGPT logic to form the RAG context.
- **Tracing**: Instrument the pipelines with Langfuse generation and retrieval tracing.
- **Verification**: Execute a batch from the UI, verify the API successfully performs retrieval, calls the LLM, and returns responses. Validate traces appear in Langfuse.

## Phase 5: Evaluation Module

Implementing the metrics for Retrieval, RAG logic, and Cognitive Tasks.

- **API**: Add evaluators for **Retrieval Metrics** (MRR, Precision@K, NDCG).
- **API**: Add evaluators for **RAG Triad** (Context Relevance, Faithfulness, Answer Relevance).
- **Architecture**: Design evaluation logic so it accepts ground-truth dataset inputs.
- **Verification**: Run a query batch alongside ground truth. Validate the API correctly computes metrics for all executed cases.

## Phase 6: Results Dashboard (Premium UI)

Visualizing the end-to-end results.

- **Frontend**: Implement modern charting using Recharts or Chart.js.
- **UI Enhancements**: Create separate cards per metric (MRR, Faithfulness, Cognitive limits like Multi-hop or Direct Fact).
- **Interpretability Layer**: Add human-readable contextual explanations for each metric score, highlighting the highest/lowest performance correctly.
- **Verification**: Thorough end-to-end testing with real configurations to ensure the UI renders the comparisons beautifully and correctly highlights the stack context for each configuration.

## Open Questions

1. Do you have a preference for the React initialization tool across the modern landscape? (Vite is recommended for single-page React apps interacting with .NET APIs).
2. For testing Phase 1, do you currently have local Docker Desktop or Podman installed to support .NET Aspire's container orchestration?

## Verification Plan

### Automated / Functional
- In Phase 1, we will verify the Aspire dashboard health checks are green.
- We will run API tests via Swagger (or `.http` files) before shifting focus to the React UI for each vertical slice.

### Manual Verification
- You will be asked to manually review the App loading state, database connection state, and the UI visually to ensure it matches the enterprise aesthetics requested in the prompt.
