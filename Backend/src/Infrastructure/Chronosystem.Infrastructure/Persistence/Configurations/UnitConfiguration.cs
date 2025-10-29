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
        builder.ToTable("units");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(u => u.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired(false);

        builder.Property(u => u.UpdatedBy)
            .HasColumnName("updated_by")
            .IsRequired(false);

        builder.Property(u => u.RowVersion)
            .HasColumnName("row_version")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
