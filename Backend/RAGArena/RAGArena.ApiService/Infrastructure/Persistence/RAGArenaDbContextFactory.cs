using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pgvector.EntityFrameworkCore;

namespace RAGArena.ApiService.Infrastructure.Persistence;

public class RAGArenaDbContextFactory : IDesignTimeDbContextFactory<RAGArenaDbContext>
{
    public RAGArenaDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<RAGArenaDbContext>();
        optionsBuilder.UseNpgsql(
            configuration.GetConnectionString("ragarena"),
            npgsqlOptions => npgsqlOptions.UseVector());

        return new RAGArenaDbContext(optionsBuilder.Options);
    }
}
