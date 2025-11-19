// ======================================================================================
// ARQUIVO: ApplicationDbContext.cs (VERSÃO AJUSTADA – Ignora RefreshToken.Tenant)
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using Chronosystem.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata; // PropertySaveBehavior
using System;

namespace Chronosystem.Infrastructure.Persistence.DbContexts;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Service> Services => Set<Service>();

    Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
        => base.SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UnitConfiguration());

        // ===== User =====
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);

            entity.Property(u => u.FullName).HasMaxLength(255).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(255).HasColumnType("citext").IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();

            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.Role).HasMaxLength(50).IsRequired();
            entity.Property(u => u.IsActive).HasDefaultValue(true).IsRequired();

            entity.Property(u => u.CreatedBy)
                  .HasColumnName("created_by");

            entity.Property(u => u.UpdatedBy)
                  .HasColumnName("updated_by");

            entity.Property(u => u.RowVersion)
                  .HasColumnName("row_version")
                  .IsConcurrencyToken()
                  .ValueGeneratedOnAddOrUpdate();

            entity.HasQueryFilter(u => u.DeletedAt == null);
        });

        // ===== RefreshToken =====
        modelBuilder.Entity<RefreshToken>().Ignore(rt => rt.CreatedBy);
        modelBuilder.Entity<RefreshToken>().Ignore(rt => rt.UpdatedBy);

        // 🔴 Ajuste mínimo: tenant não é coluna (multi-tenant por schema) → ignorar
        modelBuilder.Entity<RefreshToken>().Ignore(rt => rt.Tenant);

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(rt => rt.Id);

            entity.Property(rt => rt.TokenHash)
                  .HasMaxLength(512)
                  .IsRequired();

            entity.HasIndex(rt => rt.TokenHash).IsUnique();

            entity.Property(rt => rt.ExpiresAtUtc)
                  .HasColumnType("timestamp with time zone")
                  .IsRequired();

            entity.Property(rt => rt.IsRevoked)
                  .HasDefaultValue(false)
                  .IsRequired();

            entity.Property(rt => rt.RowVersion)
                  .IsConcurrencyToken()
                  .ValueGeneratedOnAddOrUpdate();

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("services");
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Name).HasMaxLength(255).IsRequired();
                
                // Configuração de precisão para o preço (MUITO IMPORTANTE)
                entity.Property(s => s.Price).HasPrecision(10, 2).IsRequired();
                
                entity.Property(s => s.DurationMinutes).IsRequired();

                // ✅ AQUI ESTÁ A CORREÇÃO PARA O ERRO "row_version1"
                // Mapeamos explicitamente a propriedade C# RowVersion para a coluna SQL "row_version"
                entity.Property(s => s.RowVersion)
                        .HasColumnName("row_version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                // Mapeamento explícito dos campos de auditoria específicos desta tabela
                entity.Property(s => s.CreatedBy).HasColumnName("created_by");
                entity.Property(s => s.UpdatedBy).HasColumnName("updated_by");

                entity.HasQueryFilter(s => s.DeletedAt == null);
            });

        // ===== Bloco global de auditoria (para quem herda de AuditableEntity) =====
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(Chronosystem.Domain.Common.AuditableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var builder = modelBuilder.Entity(entityType.ClrType);

            // created_at gerado no banco (DEFAULT now()), não enviar no INSERT/UPDATE
            var created = builder.Property(nameof(Chronosystem.Domain.Common.AuditableEntity.CreatedAt))
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();

            created.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            created.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            // updated_at gerado no banco no INSERT; trigger cuida no UPDATE
            var updated = builder.Property(nameof(Chronosystem.Domain.Common.AuditableEntity.UpdatedAt))
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAddOrUpdate();

            updated.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            updated.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
