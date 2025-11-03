using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Domain.Entities;

namespace Chronosystem.Application.Common.Interfaces.Persistence;

public interface IProfessionalRepository
{
    Task AddAsync(Professional professional, CancellationToken cancellationToken = default);
    Task<IEnumerable<Professional>> GetAllAsync(Guid? userId, string? registryCode, string? specialty, CancellationToken cancellationToken = default);
    Task<Professional?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Professional?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> RegistryCodeExistsAsync(string registryCode, Guid? excludeProfessionalId, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
