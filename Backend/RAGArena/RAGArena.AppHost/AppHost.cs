var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImage("pgvector/pgvector", "pg17")
    .WithPgAdmin()
    .WithDataVolume();

var ragArenaDb = postgres.AddDatabase("ragarena");

var apiService = builder.AddProject<Projects.RAGArena_ApiService>("apiservice")
    .WithReference(ragArenaDb)
    .WaitFor(ragArenaDb)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.RAGArena_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
