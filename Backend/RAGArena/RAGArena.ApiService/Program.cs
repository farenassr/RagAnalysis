using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using RAGArena.ApiService.Configuration;
using RAGArena.ApiService.Domain.Enums;
using RAGArena.ApiService.Features.Documents.Upload;
using RAGArena.ApiService.Features.Experiments.ExecuteBatch;
using RAGArena.ApiService.Infrastructure.Chunking;
using RAGArena.ApiService.Infrastructure.Embeddings;
using RAGArena.ApiService.Infrastructure.Errors;
using RAGArena.ApiService.Infrastructure.Execution;
using RAGArena.ApiService.Infrastructure.Execution.Generation;
using RAGArena.ApiService.Infrastructure.Execution.Prompting;
using RAGArena.ApiService.Infrastructure.Execution.Retrieval;
using RAGArena.ApiService.Infrastructure.Execution.Tracing;
using RAGArena.ApiService.Infrastructure.Parsers;
using RAGArena.ApiService.Infrastructure.Parsers.LlamaParse;
using RAGArena.ApiService.Infrastructure.Persistence;
using RAGArena.ApiService.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// PostgreSQL + EF Core
builder.Services.AddDbContext<RAGArenaDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("ragarena"),
        npgsqlOptions => npgsqlOptions.UseVector()));

// FastEndpoints + Swagger
builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.Title = "RAG Arena API";
            s.Version = "v1";
            s.Description = "Enterprise RAG Evaluation Platform — compare pipelines, measure quality, ship with confidence.";
        };
    });

// Global exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Configuration bindings
builder.Services.Configure<LangfuseOptions>(builder.Configuration.GetSection(LangfuseOptions.SectionName));
builder.Services.Configure<LlamaParseOptions>(builder.Configuration.GetSection(LlamaParseOptions.SectionName));
builder.Services.Configure<EmbeddingOptions>(builder.Configuration.GetSection(EmbeddingOptions.SectionName));
builder.Services.Configure<ChunkingOptions>(builder.Configuration.GetSection(ChunkingOptions.SectionName));
builder.Services.Configure<ExperimentExecutionOptions>(builder.Configuration.GetSection(ExperimentExecutionOptions.SectionName));

// Storage
builder.Services.AddSingleton<IDocumentStorageService, LocalDocumentStorageService>();

// Embedding service (single implementation for now)
builder.Services.AddScoped<IEmbeddingService, OpenAiEmbeddingService>();
builder.Services.AddScoped<IRetrievalOrchestrator, RetrievalOrchestrator>();
builder.Services.AddScoped<IPromptTemplateService, PromptTemplateService>();
builder.Services.AddScoped<ILlmAnswerService, OpenAiAnswerService>();
builder.Services.AddSingleton<ILangfuseTracingService, LangfuseTracingService>();

// Document parsers (keyed by parser type name)
builder.Services.AddHttpClient("LlamaParse", client =>
    client.BaseAddress = new Uri(builder.Configuration["LlamaParse:BaseUrl"] ?? "https://api.cloud.llamaindex.ai"));
builder.Services.AddHttpClient("OpenAIChat", client =>
    client.BaseAddress = new Uri(builder.Configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1/"));

builder.Services.AddKeyedScoped<IDocumentParserService, LlamaParseService>(DocumentParserType.LlamaParse.ToString());
// AzureDocumentIntelligence will be added in a later phase

// Chunking strategies (keyed by strategy name)
builder.Services.AddKeyedScoped<IChunkingService, SemanticChunkingService>(ChunkingStrategy.Semantic.ToString());
builder.Services.AddKeyedScoped<IChunkingService, RecursiveCharacterChunkingService>(ChunkingStrategy.RecursiveCharacter.ToString());
builder.Services.AddKeyedScoped<IChunkingService, RecursiveCharacterChunkingService>(ChunkingStrategy.DocumentAware.ToString());
builder.Services.AddKeyedScoped<IChunkingService, RecursiveCharacterChunkingService>(ChunkingStrategy.StructureAware.ToString());

// Feature handlers
builder.Services.AddScoped<UploadDocumentHandler>();
builder.Services.AddScoped<ExecuteBatchHandler>();

// CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                    ?? ["http://localhost:5173", "http://localhost:3000"])
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Apply EF schema (creates DB + tables if they don't exist)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RAGArenaDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseExceptionHandler();
app.UseCors();

app.UseFastEndpoints()
   .UseSwaggerGen();

app.MapDefaultEndpoints();

app.Run();
