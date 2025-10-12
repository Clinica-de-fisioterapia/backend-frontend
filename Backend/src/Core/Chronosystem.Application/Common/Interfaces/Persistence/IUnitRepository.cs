using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DomainUnit = Chronosystem.Domain.Units.Unit;

namespace Chronosystem.Application.Common.Interfaces.Persistence;

public interface IUnitRepository
{
    Task AddAsync(DomainUnit unit, CancellationToken cancellationToken = default);
    Task<IEnumerable<DomainUnit>> GetAllByTenantAsync(CancellationToken cancellationToken = default);
    Task<DomainUnit?> GetByIdAsync(Guid unitId, CancellationToken cancellationToken = default);
    Task<bool> UnitNameExistsAsync(string name, CancellationToken cancellationToken = default);
    void Update(DomainUnit unit);
}
