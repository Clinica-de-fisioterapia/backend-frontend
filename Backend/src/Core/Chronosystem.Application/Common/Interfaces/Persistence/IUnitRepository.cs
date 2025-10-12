using System;
using System.Collections.Generic;
using System.Threading;
using Chronosystem.Domain.Scheduling;

namespace Chronosystem.Application.Common.Interfaces.Persistence;

public interface IUnitRepository
{
    Task AddAsync(Unit unit, CancellationToken cancellationToken = default);
    Task<IEnumerable<Unit>> GetAllByTenantAsync(CancellationToken cancellationToken = default);
    Task<Unit?> GetByIdAsync(Guid unitId, CancellationToken cancellationToken = default);
    Task<bool> UnitNameExistsAsync(string name, CancellationToken cancellationToken = default);
    void Update(Unit unit);
}
