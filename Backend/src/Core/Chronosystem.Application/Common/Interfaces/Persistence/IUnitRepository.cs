// Chronosystem.Application/Common/Interfaces/Persistence/IUnitRepository.cs
using Chronosystem.Domain.Entities;

namespace Chronosystem.Application.Common.Interfaces.Persistence;

public interface IUnitRepository
{
    Task<Unit?> GetByIdAsync(Guid unitId, Guid tenantId);
    Task<IEnumerable<Unit>> GetAllByTenantAsync(Guid tenantId);
    Task AddAsync(Unit unit);
    void Update(Unit unit);
    void Remove(Unit unit);
    Task<int> SaveChangesAsync(); // Para persistir as mudanças
}