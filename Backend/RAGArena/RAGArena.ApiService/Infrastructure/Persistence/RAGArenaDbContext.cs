using Microsoft.EntityFrameworkCore;
using RAGArena.ApiService.Domain.Entities;

namespace RAGArena.ApiService.Infrastructure.Persistence;

public class RAGArenaDbContext(DbContextOptions<RAGArenaDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(512).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(128);
            entity.Property(e => e.StoragePath).HasMaxLength(1024);
            entity.Property(e => e.ParserType).HasConversion<string>();
            entity.Property(e => e.ChunkingStrategy).HasConversion<string>();
            entity.Property(e => e.EmbeddingModel).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasMany(e => e.Chunks)
                  .WithOne(c => c.Document)
                  .HasForeignKey(c => c.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Embedding).HasColumnType("vector(1536)");
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.HasIndex(e => e.DocumentId);
        });
    }
}
