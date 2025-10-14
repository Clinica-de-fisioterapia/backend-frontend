// ======================================================================================
// ARQUIVO: UserRepository.cs
// CAMADA: Infrastructure / Persistence / Repositories
// OBJETIVO: Implementa o repositório da entidade User, com suporte a multi-tenant por
//            schema e validação de usuários ativos.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Chronosystem.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // -------------------------------------------------------------------------
    // CRUD BÁSICO
    // -------------------------------------------------------------------------

    public async Task AddAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
    }

    public async Task<IEnumerable<User>> GetAllByTenantAsync(Guid tenantId)
    {
        // Em ambiente multi-tenant por schema, o tenant é resolvido via middleware
        // O parâmetro é mantido apenas por compatibilidade
        return await _dbContext.Users.AsNoTracking().ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid userId, Guid tenantId)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> UserExistsByEmailAsync(string email, Guid tenantId)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    // -------------------------------------------------------------------------
    // SUPORTE A HANDLERS
    // -------------------------------------------------------------------------

    public void Update(User user)
    {
        _dbContext.Users.Update(user);
    }

    public void Remove(User user)
    {
        _dbContext.Users.Remove(user);
    }

    public Task<int> SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Verifica se o usuário existe e está ativo dentro do schema atual.
    /// Considera também se o usuário não foi logicamente deletado.
    /// </summary>
    public async Task<bool> ExistsAndIsActiveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u =>
                u.Id == userId &&
                u.IsActive &&
                u.DeletedAt == null, cancellationToken);
    }
}
