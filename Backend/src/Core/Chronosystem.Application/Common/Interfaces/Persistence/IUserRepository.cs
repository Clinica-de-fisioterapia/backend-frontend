using Chronosystem.Domain.Entities;

namespace Chronosystem.Application.Common.Interfaces.Persistence;

public interface IUserRepository
{
    // -------------------------------------------------------------------------
    // CRUD BÁSICO
    // -------------------------------------------------------------------------
    Task AddAsync(User user);
    Task<IEnumerable<User>> GetAllByTenantAsync(Guid tenantId);
    Task<User?> GetByIdAsync(Guid userId, Guid tenantId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> UserExistsByEmailAsync(string email, Guid tenantId);

    // -------------------------------------------------------------------------
    // SUPORTE A HANDLERS
    // -------------------------------------------------------------------------
    void Update(User user);
    void Remove(User user);
    Task<int> SaveChangesAsync();

    // -------------------------------------------------------------------------
    // NOVOS MÉTODOS DE VALIDAÇÃO (para Units, etc.)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifica se existe um usuário ativo com o ID informado dentro do tenant atual.
    /// </summary>
    /// <param name="userId">Identificador do usuário.</param>
    /// <param name="cancellationToken">Token opcional de cancelamento.</param>
    /// <returns>Retorna true se o usuário existir e estiver ativo.</returns>
    Task<bool> ExistsAndIsActiveAsync(Guid userId, CancellationToken cancellationToken = default);
}
