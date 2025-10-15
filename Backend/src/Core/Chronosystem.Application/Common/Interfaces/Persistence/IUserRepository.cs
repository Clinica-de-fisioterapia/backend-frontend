// ======================================================================================
// ARQUIVO: IUserRepository.cs
// CAMADA: Application / Common / Interfaces / Persistence
// OBJETIVO: Define o contrato para o repositório de usuários (Users). 
//            Compatível com multi-tenant por schema (sem TenantId explícito).
// ======================================================================================

using Chronosystem.Domain.Entities;

namespace Chronosystem.Application.Common.Interfaces.Persistence;

public interface IUserRepository
{
    // -------------------------------------------------------------------------
    // CRUD BÁSICO
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adiciona um novo usuário ao contexto atual.
    /// </summary>
    Task AddAsync(User user);

    /// <summary>
    /// Retorna todos os usuários ativos do tenant atual.
    /// </summary>
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna um usuário pelo seu ID dentro do schema atual.
    /// </summary>
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna um usuário pelo e-mail dentro do schema atual.
    /// </summary>
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe algum usuário com o e-mail informado (não deletado).
    /// </summary>
    Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // SUPORTE A HANDLERS / PERSISTÊNCIA
    // -------------------------------------------------------------------------

    void Update(User user);
    void Remove(User user);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // NOVAS VALIDAÇÕES / UTILIDADES
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifica se existe um usuário ativo com o ID informado dentro do schema atual.
    /// </summary>
    /// <param name="userId">Identificador do usuário.</param>
    /// <param name="cancellationToken">Token opcional de cancelamento.</param>
    /// <returns>Retorna true se o usuário existir e estiver ativo.</returns>
    Task<bool> ExistsAndIsActiveAsync(Guid userId, CancellationToken cancellationToken = default);
}
