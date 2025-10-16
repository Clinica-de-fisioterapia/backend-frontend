// ======================================================================================
// ARQUIVO: ApplicationDbContext.cs
// CAMADA: Infrastructure / Persistence / DbContexts
// OBJETIVO: Contexto principal do EF Core para o schema do tenant atual.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using Chronosystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Reflection;

namespace Chronosystem.Infrastructure.Persistence.DbContexts;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Unit> Units => Set<Unit>();

    Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
        => base.SaveChangesAsync(cancellationToken);

    // -------------------------------------------------------------------------
    // ⚙️ Configuração de conexão e enum mapping
    // -------------------------------------------------------------------------
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(
                "Host=localhost;Port=5432;Database=chronosystem;Username=postgres;Password=1234"
            );

            // ✅ Mapeia o Enum C# → Enum PostgreSQL real
            dataSourceBuilder.MapEnum<UserRole>("public.user_role");

            var dataSource = dataSourceBuilder.Build();
            optionsBuilder.UseNpgsql(dataSource)
                          .UseSnakeCaseNamingConvention();
        }
    }

    // -------------------------------------------------------------------------
    // 🧱 Mapeamentos
    // -------------------------------------------------------------------------
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ✅ Registro do Enum PostgreSQL
        modelBuilder.HasPostgresEnum<UserRole>("public", "user_role");

        // ---------------------------------------------------------------------
        // USERS
        // ---------------------------------------------------------------------
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);

            entity.Property(u => u.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();

            // 🧩 CORREÇÃO: converte automaticamente enum C# → string literal do ENUM PostgreSQL
            entity.Property(u => u.Role)
                .HasColumnName("role")
                .HasConversion<string>() // <--- ESSENCIAL!
                .HasColumnType("public.user_role")
                .IsRequired();

            entity.Property(u => u.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            // Auditoria
            entity.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("TIMESTAMP WITH TIME ZONE")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(u => u.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("TIMESTAMP WITH TIME ZONE");

            entity.Property(u => u.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("TIMESTAMP WITH TIME ZONE");

            // Concorrência otimista
            entity.Property(u => u.RowVersion)
                .HasColumnName("row_version")
                .IsConcurrencyToken();

            // Soft delete
            entity.HasQueryFilter(u => u.DeletedAt == null);
        });

        // ---------------------------------------------------------------------
        // REFRESH TOKENS
        // ---------------------------------------------------------------------
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(rt => rt.Id);

            entity.Property(rt => rt.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(rt => rt.TokenHash).HasColumnName("token_hash").HasMaxLength(256).IsRequired();
            entity.Property(rt => rt.ExpiresAtUtc).HasColumnName("expires_at_utc").HasColumnType("TIMESTAMP WITH TIME ZONE").IsRequired();
            entity.Property(rt => rt.IsRevoked).HasColumnName("is_revoked").HasDefaultValue(false);
            entity.Property(rt => rt.CreatedAt).HasColumnName("created_at").HasColumnType("TIMESTAMP WITH TIME ZONE").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(rt => rt.DeletedAt == null);
        });

        // ---------------------------------------------------------------------
        // Configurações adicionais
        // ---------------------------------------------------------------------
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
