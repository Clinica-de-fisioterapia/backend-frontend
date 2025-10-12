using Chronosystem.Domain.Entities;

namespace Chronosystem.Application.Common.Interfaces.Persistence;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    void Update(User user);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
