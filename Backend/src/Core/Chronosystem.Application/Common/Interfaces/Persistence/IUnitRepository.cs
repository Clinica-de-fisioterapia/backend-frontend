
using Chronosystem.Domain.Entities;

namespace Chronosystem.Application.Common.Interfaces.Persistence;

public interface IUnitRepository
{
    Task<Unit?> GetByIdAsync(Guid unitId);
    Task<IEnumerable<Unit>> GetAllByTenantAsync();
    Task AddAsync(Unit unit);
    void Update(Unit unit);
    void Remove(Unit unit);
    Task<bool> UnitNameExistsAsync(string name); 
}