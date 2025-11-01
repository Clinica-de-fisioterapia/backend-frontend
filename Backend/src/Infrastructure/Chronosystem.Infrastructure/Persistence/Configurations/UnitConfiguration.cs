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
        builder.ToTable("units");

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
        // CAMPOS DE AUDITORIA (forte)
        // -------------------------------------------------------------------------
        builder.Property(u => u.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(u => u.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(u => u.RowVersion)
            .HasColumnName("row_version")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
