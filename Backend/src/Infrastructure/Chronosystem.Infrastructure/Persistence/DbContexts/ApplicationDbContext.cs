// ======================================================================================
// ARQUIVO: ApplicationDbContext.cs (VERSÃO AJUSTADA – AUDITORIA + ROW_VERSION)
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata; // << necessário para PropertySaveBehavior
using System;
using System.Reflection;

namespace Chronosystem.Infrastructure.Persistence.DbContexts;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Unit> Units => Set<Unit>();

    Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
        => base.SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mantém ignorados (colunas podem existir no banco, mas não serão persistidas pela API)
        modelBuilder.Entity<User>().Ignore(u => u.CreatedBy);
        modelBuilder.Entity<User>().Ignore(u => u.UpdatedBy);

        // -----------------------------
        // User mapping (inalterado, exceto row_version)
        // -----------------------------
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);

            entity.Property(u => u.FullName).HasMaxLength(255).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(255).HasColumnType("citext").IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();

            entity.HasIndex(u => u.Email).IsUnique();

            // Role como TEXT puro (extensível). Validação por regex na camada Application.
            entity.Property(u => u.Role).HasMaxLength(50).IsRequired();

            entity.Property(u => u.IsActive).HasDefaultValue(true).IsRequired();

            // 🔁 Concorrência: valor gerado no banco (DEFAULT no insert + trigger no update)
            entity.Property(u => u.RowVersion)
                  .IsConcurrencyToken()
                  .ValueGeneratedOnAddOrUpdate();

            entity.HasQueryFilter(u => u.DeletedAt == null);
        });

        // ---------------------------------------------------------
        // 🔐 BLOCO GLOBAL DE AUDITORIA (para quem herda de AuditableEntity)
        // - Não envia CreatedAt/UpdatedAt no INSERT/UPDATE (deixa o banco cuidar)
        // - Tipagem consistente com TIMESTAMPTZ
        // ---------------------------------------------------------
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(Chronosystem.Domain.Common.AuditableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var builder = modelBuilder.Entity(entityType.ClrType);

            // created_at: DEFAULT now() no INSERT (não enviar pelo EF)
            var created = builder.Property(nameof(Chronosystem.Domain.Common.AuditableEntity.CreatedAt))
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();

            created.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            created.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // updated_at: DEFAULT now() no INSERT; trigger atualiza no UPDATE (não enviar pelo EF)
            var updated = builder.Property(nameof(Chronosystem.Domain.Common.AuditableEntity.UpdatedAt))
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAddOrUpdate();

            updated.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            updated.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
