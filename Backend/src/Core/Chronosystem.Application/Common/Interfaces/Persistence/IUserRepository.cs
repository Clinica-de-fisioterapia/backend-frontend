using Chronosystem.Domain.Entities;

namespace Chronosystem.Application.Common.Interfaces.Persistence;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<IEnumerable<User>> GetAllByTenantAsync(Guid tenantId);
    Task<User?> GetByIdAsync(Guid userId, Guid tenantId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> UserExistsByEmailAsync(string email, Guid tenantId);

    // Métodos adicionados para suportar os Handlers
    void Update(User user);
    void Remove(User user);
    Task<int> SaveChangesAsync();
}
