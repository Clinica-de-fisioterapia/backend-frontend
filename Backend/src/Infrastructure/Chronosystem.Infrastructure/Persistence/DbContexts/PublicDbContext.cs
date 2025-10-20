using Chronosystem.Domain.Entities; // Supondo que a entidade Tenant esteja aqui
using Microsoft.EntityFrameworkCore;

namespace Chronosystem.Infrastructure.Persistence.DbContexts;

// Este DbContext só se preocupa com o esquema 'public'.
public class PublicDbContext : DbContext
{
    public PublicDbContext(DbContextOptions<PublicDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    // public DbSet<Plan> Plans => Set<Plan>(); // Se a tabela 'plans' também for pública

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Garante que este contexto sempre use o esquema 'public'
        modelBuilder.HasDefaultSchema("public");

        // Configuração da entidade Tenant
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            // ... outras configurações se necessário
        });
    }
}