using Encurtador.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Encurtador.Api.Infrastructure.Data;

public class EncurtadorDbContext : DbContext
{
    public EncurtadorDbContext(DbContextOptions<EncurtadorDbContext> options)
        : base(options)
    {
    }

    public DbSet<Link> Links => Set<Link>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Link>(entity =>
        {
            entity.ToTable("Links");
            entity.HasKey(link => link.Id);
            entity.Property(link => link.Code).IsRequired().HasMaxLength(32);
            entity.Property(link => link.TargetUrl).IsRequired().HasMaxLength(2048);
            entity.Property(link => link.ManagementKeyHash).IsRequired().HasMaxLength(128);
            entity.HasIndex(link => link.Code).IsUnique();
        });
    }
}
