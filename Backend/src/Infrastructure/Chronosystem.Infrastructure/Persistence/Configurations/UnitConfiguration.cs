// ======================================================================================
// ARQUIVO: UnitConfiguration.cs
// CAMADA: Infrastructure / Persistence / Configurations
// OBJETIVO: Define o mapeamento entre a entidade Unit (Domain) e a tabela "units".
//            Compatível com multi-tenant por schema (sem TenantId) e com suporte a
//            auditoria e timezone UTC conforme o Guia Banco V2.
// ======================================================================================

using Chronosystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chronosystem.Infrastructure.Persistence.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        // -------------------------------------------------------------------------
        // NOME DA TABELA
        // -------------------------------------------------------------------------
        // O EFCore.NamingConventions já converte automaticamente "Unit" → "units",
        // portanto não é necessário especificar builder.ToTable("units").
        // Isso mantém compatibilidade com múltiplos schemas de tenants.
        // -------------------------------------------------------------------------

        // -------------------------------------------------------------------------
        // CHAVE PRIMÁRIA
        // -------------------------------------------------------------------------
        builder.HasKey(u => u.Id);

        // O Id será gerado automaticamente pelo PostgreSQL usando gen_random_uuid()
        builder.Property(u => u.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        // -------------------------------------------------------------------------
        // CAMPOS PRINCIPAIS
        // -------------------------------------------------------------------------
        builder.Property(u => u.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        // -------------------------------------------------------------------------
        // CAMPOS DE AUDITORIA
        // -------------------------------------------------------------------------
        // Esses campos são gerenciados pelo PostgreSQL (triggers automáticas).
        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at")
            .ValueGeneratedOnAddOrUpdate(); // trigger define CURRENT_TIMESTAMP no soft delete
    }
}
