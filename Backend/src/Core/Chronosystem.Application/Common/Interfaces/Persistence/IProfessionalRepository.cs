using Chronosystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Common.Interfaces.Persistence
{
    public interface IProfessionalRepository
    {
        Task<Professional?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Professional>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Professional entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(Professional entity, CancellationToken cancellationToken = default);

        // útil para validação (person_id único)
        Task<bool> ExistsByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);
    }
}
