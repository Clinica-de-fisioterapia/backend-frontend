// ======================================================================================
// ARQUIVO: UserRepository.cs
// CAMADA: Infrastructure / Persistence / Repositories
// OBJETIVO: Implementa o repositório da entidade User, com suporte a multi-tenant por
//            schema e validação de usuários ativos (sem TenantId explícito).
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

    /// <summary>
    /// Adiciona um novo usuário ao contexto atual.
    /// </summary>
    public async Task AddAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
    }

    /// <summary>
    /// Retorna todos os usuários ativos do schema atual.
    /// </summary>
    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.DeletedAt == null)
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retorna um usuário pelo seu ID dentro do schema atual.
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken);
    }

    /// <summary>
    /// Retorna um usuário pelo e-mail dentro do schema atual.
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == email.ToLower() &&
                u.DeletedAt == null, cancellationToken);
    }

    /// <summary>
    /// Verifica se existe algum usuário ativo com o e-mail informado.
    /// </summary>
    public async Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u =>
                u.Email.ToLower() == email.ToLower() &&
                u.DeletedAt == null, cancellationToken);
    }

    // -------------------------------------------------------------------------
    // SUPORTE A HANDLERS / PERSISTÊNCIA
    // -------------------------------------------------------------------------

    public void Update(User user)
    {
        _dbContext.Users.Update(user);
    }

    public void Remove(User user)
    {
        _dbContext.Users.Remove(user);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    // -------------------------------------------------------------------------
    // NOVAS VALIDAÇÕES / UTILIDADES
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifica se o usuário existe, está ativo e não foi logicamente deletado.
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
