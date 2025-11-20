using Chronosystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chronosystem.Infrastructure.Persistence.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("customers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PersonId)
                .IsRequired();

            builder.HasOne(x => x.Person)
                .WithOne()
                .HasForeignKey<Customer>(x => x.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.RowVersion)
                .IsRowVersion()
                .HasColumnName("row_version");
        }
    }
}
