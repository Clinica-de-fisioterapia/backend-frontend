// Chronosystem.Application/Common/Interfaces/Persistence/IUserRepository.cs
using Chronosystem.Domain.Entities;

namespace Chronosystem.Application.Common.Interfaces.Persistence;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid userId, Guid tenantId);
    Task<IEnumerable<User>> GetAllByTenantAsync(Guid tenantId);
    Task AddAsync(User user);
    Task<bool> UserExistsByEmailAsync(string email, Guid tenantId);
    Task<int> SaveChangesAsync();
}