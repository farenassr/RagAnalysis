# Agent Prompt — RAG Evaluation Arena

## Role

Act as a **Software Architect**, **Senior AI Engineer**, and **Senior Full-Stack Engineer**.

We are building an enterprise-grade full-stack platform called **RAG Evaluation Arena**. Its purpose is to let users configure, execute, evaluate, and compare multiple **RAG (Retrieval-Augmented Generation) pipelines** side by side using the same uploaded files, so performance can be measured empirically across different configurations.

You must think like a senior engineer designing a real product, not a toy demo.

---

## Mission

Design and help implement a production-style application that allows a user to:

- upload one or more files once
- configure multiple RAG experiment cases in a single workspace
- run all experiment cases against the same uploaded files
- compare results visually in a premium dashboard
- inspect which stack/configuration produced each result
- evaluate retrieval quality, answer quality, and reasoning-category performance

---

## Mandatory Tech Stack

Use these technologies and patterns strictly unless there is a strong technical reason not to:

### Backend (Use BackEnd folder)

- **.NET 10** (net10.0) using the latest C#
- **.NET Aspire 13.2.2** (`Aspire.AppHost.Sdk` + `Aspire.Hosting.AppHost`) for orchestration, distributed configuration, and telemetry
- **FastEndpoints 8.1.0** for APIs
- **Vertical Slice Architecture**
- **Global Error Handling middleware**
- **Entity Framework Core 10.0.6** with `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1`
- **AOT-aware design**, but do **not** sacrifice EF Core compatibility just to force strict Native AOT
- Use **dependency injection dictionary/patterns** if needed so a single abstraction can switch between multiple user-selected implementations
- **General rules** create well descripted variable names, classes, dto, avoid redundant code, keep it simple. Use names for the dtos and endpoints classes like: CreateEmbeddingDto, GetEmbeddingdto, CreateEmbedingEndpoint.cs, etc.

### Database

- **PostgreSQL**
- **pgvector**

### Frontend (Use FrontEnd folder)

- **React**
- **TypeScript**
- **Kubb** for OpenAPI/Swagger-based API client generation
- **TanStack Query** for server state
- **Zustand** for client/UI/app state only
- **React Hook Form** for forms
- **Zod** for schema validation
- Use modern charting libraries such as **Recharts** or **Chart.js**

### Observability

- **OpenTelemetry 1.15.x** (`OpenTelemetry.Extensions.Hosting`, `Instrumentation.AspNetCore`, `Instrumentation.Http`, `Instrumentation.Runtime`)
- Deep integration with **.NET Aspire telemetry**
- **Langfuse** for LLM tracing, prompt inspection, latency, and RAG step inspection
- **Azure AI telemetry**

### Testing

- **Do not create unit tests for now**

---

## Product Goal

This application is a **RAG experimentation and evaluation arena**.

The user must be able to create and compare multiple RAG pipelines by selecting different combinations of:

- document parser / ingestion technology
- chunking strategy
- embedding model
- retrieval technique
- reranker
- optional safety mode such as PII masking
- database target name
- prompt strategy if needed

The application should then run all selected cases and present results in a premium, comparison-first dashboard.

---

## Supported Options

### Document Parsers / Ingestion Technologies

- **LlamaParse**
- **Azure Document Intelligence**

### Chunking Techniques

- **Semantic Chunking**
- **Document-Aware / Structural Chunking**
- **Recursive Character Chunking**
- **Structure-Aware Chunking**

### Embedding Models

- **OpenAI**
  - `text-embedding-3-small`
  - `text-embedding-3-large`
- **Microsoft / Azure OpenAI embeddings**

### Retrieval Techniques

- **Dense Vector Retrieval** using pgvector
- **BM25**
- **Hybrid Retrieval**
- **Hybrid Retrieval + RRF**
- **Azure AI Search**
- **Query Rewriting**
- **Query Expansion**
- **HyDE**
- **Re-ranking Pipeline**

### Re-rankers

- **Cohere Rerank**
- **BGE-Reranker** using Azure AI only

### Database

- **PostgreSQL with pgvector**

### LLM

- **ChatGPT** for now

### Security Option

- **PII Masking** during ingestion

---

## Core UX Requirement — Single Workspace

The application must use a **single main tab/workspace** for experiment setup.

This single workspace must allow the user to:

- upload the source file(s) once
- configure multiple experiment cases/comparisons
- run all experiment cases together
- later inspect the results in a premium visual dashboard

Avoid forcing the user through multiple setup tabs.

---

## Shared File Upload

There must be **one shared file upload component** at the top of the workspace.

The uploaded files are the common knowledge source for all experiment cases in the current run.

### Required behavior

- uploaded files are shared across all experiment cases
- the user should not upload the same files repeatedly
- all experiment cases in a batch must run against the same uploaded source set for fair comparison

---

## Multi-Case Configuration Builder

Below the shared upload area, show a configurable experiment builder.

### Default behavior

- show **one experiment case** by default

### Each case must include configurable selectors/dropdowns for:

- chunking technique
- retrieval technique
- embedding model
- reranker
- prompt template if applicable
- evaluation framework if applicable
- database / vector store type
- database name
- any additional relevant options

### User actions required

The user must be able to:

1. click **Add Comparison** or **Add Row**
2. create multiple configuration rows/cards
3. see the same available options repeated in each new row
4. configure each row independently
5. choose a different **database name** per row if desired
6. remove rows
7. clearly identify each case, for example:
   - Case A
   - Case B
   - Case C

The layout must stay elegant and readable even when multiple rows are added.

---

## Run Behavior

When the user clicks **Run**:

- validate the shared upload and all experiment rows
- collect all configured cases
- show a strong loading/progress state
- execute all selected experiment cases
- track batch progress
- ideally track per-case progress too
- persist experiment runs and their results for later review

Do not make this feel like a toy demo. It must feel like a real experimentation platform.

---

## Evaluation Requirements

The application must evaluate and compare each case using multiple dimensions.

---

## A. Retrieval Metrics

Include at minimum:

- **MRR**
- **Precision@K**
- **NDCG**

For each one:

- calculate the score
- visualize it in its own chart
- explain what it measures
- explain how to interpret high vs low values

---

## B. RAG Triad (End-to-End Evaluation)

Include:

- **Context Relevance**
- **Faithfulness / Groundedness**
- **Answer Relevance**

For each one:

- calculate the score
- visualize it in its own chart
- explain what it measures
- explain why it matters in a RAG system

---

## C. Cognitive Test Categories

Support evaluation datasets and questions categorized by reasoning type:

- **Direct Fact**
- **Temporal**
- **Spanning / Multi-hop**
- **Comparative**
- **Numerical**
- **Relationship**
- **Holistic**

For each category:

- visualize performance in its own chart/card
- explain what kind of reasoning it tests
- help the user understand which configurations perform better for that reasoning type

---

## Evaluation Framework Architecture

Design the architecture so experiment runs can be evaluated using frameworks such as:

- **Langfuse**
- **Azure AI Prompt Flow**

For IR/retrieval metrics, use logic comparable to:

- **ranx**
- **beir**

The architecture must be ready for those integrations even if not every integration is fully implemented in the first iteration.

---

## Ground Truth / Dataset Support

The system must allow loading a **question dataset / ground truth dataset** categorized by reasoning type so the user can identify where a pipeline fails.

The platform should support analysis by:

- metric
- experiment case
- reasoning category
- stack configuration

---

## Results Dashboard

The results experience must feel like a **premium analytics dashboard**, visually inspired by high-end comparison dashboards.

### Style direction

- clean card-based layout
- elegant spacing
- premium minimal aesthetic
- polished typography
- strong visual clarity
- comparison-first design
- suitable for portfolio, interviews, or enterprise demos

It should **not** look like a rough developer debug screen.

---

## Chart Requirements

Show **one chart per metric/test**.

### Examples

- one chart for MRR
- one chart for Precision@K
- one chart for NDCG
- one chart for Context Relevance
- one chart for Faithfulness / Groundedness
- one chart for Answer Relevance
- one chart for Direct Fact
- one chart for Temporal
- one chart for Spanning / Multi-hop
- one chart for Comparative
- one chart for Numerical
- one chart for Relationship
- one chart for Holistic

Each chart must compare all user-created cases side by side.

---

## Metric Card Design

Each metric card/chart should include:

- title at top-left
- small subtitle/description under the title
- a bar chart or other suitable comparison chart
- x-axis showing experiment cases
- a clean ranking/comparison feel
- subtle card styling with border/radius/shadow if appropriate

For every metric card:

- show the metric/test name prominently
- show a short human explanation of what it measures
- indicate whether **higher is better** or **lower is better** where relevant
- keep case labels consistent across charts

---

## Stack / Configuration Display

For every result, show in smaller text the **stack/configuration** used by that case.

For example, under or near the case label, show a compact summary such as:

- parser used
- chunking strategy
- embedding model
- retrieval strategy
- reranker
- prompt template
- database/store used

This text should be secondary but visible enough so the user can immediately connect a score with the exact stack that produced it.

---

## Result Interpretation Layer

Do **not** show only charts.

The UI must also explain each metric/test in plain language.

For every metric or reasoning category shown, include:

- what it measures
- why it matters
- how to interpret higher or lower values
- what kind of failure it helps detect in a RAG system

The application should educate the user while comparing cases.

---

## Dashboard Sections

Organize the results page into sections such as:

### 1. Highlights

- top-performing case overall
- fastest case
- most accurate case
- best cost/performance case if applicable
- use eye catching icons and animations for this.

### 2. Retrieval Metrics

- MRR
- Precision@K
- NDCG

### 3. RAG Quality Metrics

- Context Relevance
- Faithfulness / Groundedness
- Answer Relevance

### 4. Cognitive Performance

- Direct Fact
- Temporal
- Spanning / Multi-hop
- Comparative
- Numerical
- Relationship
- Holistic

### 5. Detailed Case Breakdown

- full configuration per case
- detailed scores
- retrieved chunks
- prompts
- outputs
- reranked results

---

## Architectural Expectations

Design the system so it is:

- modular
- extensible
- enterprise-friendly
- comparison-first
- observability-rich
- cleanly separated by vertical slices
- realistic to implement iteratively

Do not overcomplicate the design just for theory. Prefer practical, maintainable architecture.

---

## Execution Strategy for the AI

Do **not** generate the whole application at once.

Work iteratively.

### Generate the **base solution structure** with:

- .NET Aspire
- API project
- React frontend
- PostgreSQL integration
- main dependencies
- initial project references
- OpenTelemetry and Langfuse foundation

Then stop and confirm when it is ready.

### Implement the **first Vertical Slice**, for example:

- document ingestion with **LlamaParse**
- semantic chunking
- storing chunks in PostgreSQL/pgvector

Then stop and wait for approval.

---

## Output Rules

When responding:

- be concrete and implementation-oriented
- avoid generic fluff
- prefer practical architecture decisions
- use clean naming
- keep the structure easy to follow
- where useful, include:
  - folder structure
  - project structure
  - API endpoint proposals
  - DTOs
  - entities
  - Mermaid diagrams
  - React component structure
  - C# interfaces
  - README file

Do not skip important implementation details.

---
