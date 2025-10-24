// ======================================================================================
// ARQUIVO: ApplicationDbContext.cs (VERSÃO FINAL E CORRETA)
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
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
        modelBuilder.Entity<User>().Ignore(u => u.CreatedBy);
        modelBuilder.Entity<User>().Ignore(u => u.UpdatedBy);

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
            entity.Property(u => u.RowVersion).IsConcurrencyToken();
            entity.HasQueryFilter(u => u.DeletedAt == null);
        });
    }
}