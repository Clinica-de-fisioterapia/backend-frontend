using Chronosystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chronosystem.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(s => s.DurationMinutes)
            .IsRequired();

        // Mapeamento para numeric(10, 2)
        builder.Property(s => s.Price)
            .HasPrecision(10, 2)
            .IsRequired();

        // Concorrência otimista
        builder.Property(s => s.RowVersion)
            .IsConcurrencyToken();

        // Soft Delete
        builder.HasQueryFilter(s => s.DeletedAt == null);
    }
}