// Backend/src/Infrastructure/Chronosystem.Infrastructure/Persistence/DbContexts/ApplicationDbContext.cs

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using DomainUnit = Chronosystem.Domain.Units.Unit;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using EFCore.NamingConventions;

// ATENÇÃO: Verifique se este namespace está 100% correto com sua estrutura de pastas.
namespace Chronosystem.Infrastructure.Persistence.DbContexts;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // --- DbSets ---
    // Apenas o DbSet para Unit está ativo para o teste do primeiro CRUD.
    public DbSet<DomainUnit> Units { get; set; }
    public DbSet<User> Users { get; set; }

    // Todas as outras entidades foram comentadas para evitar erros de compilação.
    // Você pode descomentá-las à medida que for criando as respectivas classes no Domain.
    /*
    public DbSet<Plan> Plans { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
    public DbSet<Person> People { get; set; }
    public DbSet<Professional> Professionals { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<ScheduleException> ScheduleExceptions { get; set; }
    public DbSet<ProfessionalSchedule> ProfessionalSchedules { get; set; }
    public DbSet<TenantSetting> TenantSettings { get; set; }
    */



    Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Este método é chamado pelo Entity Framework para configurar o modelo.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Esta linha continuará funcionando e aplicará apenas as configurações que encontrar,
        // que no momento será apenas a 'UnitConfiguration'.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresEnum<UserRole>();
    }

    // Chronosystem.Infrastructure/Persistence/DbContexts/ApplicationDbContext.cs
    
}