using System.Diagnostics;

namespace RAGArena.ApiService.Infrastructure.Execution;

public static class ExecutionActivitySource
{
    public static readonly ActivitySource Instance = new("RAGArena.Execution");
}
