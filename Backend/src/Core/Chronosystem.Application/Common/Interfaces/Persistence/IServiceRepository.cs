using Chronosystem.Domain.Entities;

namespace Chronosystem.Application.Common.Interfaces.Persistence;

public interface IServiceRepository
{
    Task AddAsync(Service service, CancellationToken ct);
    Task<Service?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<Service>> GetAllAsync(CancellationToken ct);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct);
    void Update(Service service);
    Task SaveChangesAsync(CancellationToken ct);
}