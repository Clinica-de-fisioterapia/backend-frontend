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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação do repositório de usuários responsável por executar operações
/// de leitura e escrita dentro do schema ativo (multi-tenant).
/// </summary>
/// <remarks>
/// Todas as consultas e comandos são automaticamente aplicados ao schema do tenant atual,
/// definido pelo middleware <c>X-Tenant</c> via <c>ApplicationDbContext</c>.
/// </remarks>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Inicializa uma nova instância do repositório de usuários.
    /// </summary>
    /// <param name="dbContext">Contexto de banco de dados configurado com o schema do tenant atual.</param>
    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // -------------------------------------------------------------------------
    // 🧱 CRUD BÁSICO
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adiciona um novo usuário ao contexto atual.
    /// </summary>
    /// <param name="user">Entidade <see cref="User"/> a ser adicionada.</param>
    public async Task AddAsync(User user) =>
        await _dbContext.Users.AddAsync(user);

    /// <summary>
    /// Retorna todos os usuários ativos (não deletados) do schema atual.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Coleção de entidades <see cref="User"/>.</returns>
    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.DeletedAt == null)
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Busca um usuário específico pelo seu identificador único.
    /// </summary>
    /// <param name="userId">Identificador do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>
    /// A entidade <see cref="User"/> correspondente, ou <c>null</c> se não for encontrada.
    /// </returns>
    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken);

    /// <summary>
    /// Busca um usuário pelo endereço de e-mail (case insensitive).
    /// </summary>
    /// <param name="email">Endereço de e-mail do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>
    /// A entidade <see cref="User"/> correspondente, ou <c>null</c> se não for encontrada.
    /// </returns>
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();

        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Email == normalized &&
                u.DeletedAt == null, cancellationToken);
    }

    /// <summary>
    /// Verifica se já existe um usuário ativo com o e-mail informado.
    /// </summary>
    /// <param name="email">Endereço de e-mail a verificar.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns><c>true</c> se o e-mail já estiver em uso; caso contrário, <c>false</c>.</returns>
    public async Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();

        return await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u =>
                u.Email == normalized &&
                u.DeletedAt == null, cancellationToken);
    }

    // -------------------------------------------------------------------------
    // 🧭 PERSISTÊNCIA
    // -------------------------------------------------------------------------

    /// <summary>
    /// Atualiza o estado da entidade <see cref="User"/> dentro do contexto atual.
    /// </summary>
    /// <param name="user">Entidade a ser atualizada.</param>
    public void Update(User user) =>
        _dbContext.Users.Update(user);

    /// <summary>
    /// Remove permanentemente a entidade <see cref="User"/> do contexto.
    /// ⚠️ Use apenas em operações administrativas — o padrão é soft delete.
    /// </summary>
    /// <param name="user">Entidade a ser removida.</param>
    public void Remove(User user) =>
        _dbContext.Users.Remove(user);

    /// <summary>
    /// Persiste todas as alterações pendentes no contexto de banco de dados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Número de registros afetados.</returns>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    // -------------------------------------------------------------------------
    // 🧩 UTILITÁRIOS
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifica se o usuário existe, está ativo e não foi logicamente deletado.
    /// </summary>
    /// <param name="userId">Identificador do usuário a ser verificado.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>
    /// <c>true</c> se o usuário existir e estiver ativo; caso contrário, <c>false</c>.
    /// </returns>
    public async Task<bool> ExistsAndIsActiveAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u =>
                u.Id == userId &&
                u.IsActive &&
                u.DeletedAt == null, cancellationToken);
}
