using System;
using Chronosystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chronosystem.Infrastructure.Persistence.DbContexts;

/// <summary>
/// DbContext bound to the global public schema (catalog tables).
/// Use this context only for cross-tenant, public schema data (e.g., tenant catalog).
/// </summary>
public class PublicDbContext : DbContext
{
    public PublicDbContext(DbContextOptions<PublicDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ----- TENANTS (public.tenants) -----
        modelBuilder.Entity<Tenant>(entity =>
        {
            // Table mapping: schema public, table tenants
            entity.ToTable("tenants", schema: "public");

            // Primary key: use inherited Id and map to column "tenant_id"
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id)
                  .HasColumnName("tenant_id")
                  .IsRequired();

            entity.Property(x => x.Slug)
                  .HasColumnName("slug")
                  .IsRequired();

            entity.Property(x => x.Name)
                  .HasColumnName("name")
                  .IsRequired();

            entity.Property(x => x.IsActive)
                  .HasColumnName("is_active")
                  .IsRequired();

            // Auditable columns (keep if base doesn't already configure them)
            entity.Property<DateTime>("created_at").HasColumnName("created_at");
            entity.Property<DateTime?>("updated_at").HasColumnName("updated_at");
            entity.Property<DateTime?>("deleted_at").HasColumnName("deleted_at");

            // Unique index on slug (matches DB)
            entity.HasIndex(x => x.Slug).IsUnique();
        });
    }
}
