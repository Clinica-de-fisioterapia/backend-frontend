using Chronosystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chronosystem.Infrastructure.Persistence.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        // A linha builder.ToTable("Units") foi REMOVIDA.
        // Agora, o EFCore.NamingConventions irá converter automaticamente
        // o nome da classe 'Unit' para o nome da tabela 'units' em snake_case.

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .HasMaxLength(255)
            .IsRequired();

        // Informa ao EF Core que o valor para 'CreatedAt' é gerado pelo banco.
        builder.Property(u => u.CreatedAt)
            .ValueGeneratedOnAdd();

        // Informa ao EF Core que 'UpdatedAt' é gerado pelo banco ao adicionar OU atualizar.
        builder.Property(u => u.UpdatedAt)
            .ValueGeneratedOnAddOrUpdate();
    }
}