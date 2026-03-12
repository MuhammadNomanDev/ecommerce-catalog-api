using EcommerceCatalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCatalog.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Description)
                .HasMaxLength(2000);

            entity.Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.ImageUrl)
                .HasMaxLength(500);

            // Index for common queries
            entity.HasIndex(p => p.IsActive);
            entity.HasIndex(p => p.CreatedAt);
        });
    }
}
