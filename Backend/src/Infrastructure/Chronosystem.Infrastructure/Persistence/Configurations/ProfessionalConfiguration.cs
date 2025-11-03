using Chronosystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chronosystem.Infrastructure.Persistence.Configurations;

public sealed class ProfessionalConfiguration : IEntityTypeConfiguration<Professional>
{
    public void Configure(EntityTypeBuilder<Professional> builder)
    {
        builder.ToTable("professionals");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.RegistryCode)
            .HasColumnName("registry_code")
            .HasColumnType("citext")
            .HasMaxLength(100);

        builder.Property(p => p.Specialty)
            .HasColumnName("specialty")
            .HasMaxLength(150);

        builder.Property(p => p.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(p => p.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(p => p.RowVersion)
            .HasColumnName("row_version")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(p => p.UserId)
            .IsUnique()
            .HasDatabaseName("ux_professionals_user_id");

        builder.HasIndex(p => p.RegistryCode)
            .IsUnique()
            .HasFilter("registry_code IS NOT NULL")
            .HasDatabaseName("ux_professionals_registry_code");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(p => p.DeletedAt == null);
    }
}
