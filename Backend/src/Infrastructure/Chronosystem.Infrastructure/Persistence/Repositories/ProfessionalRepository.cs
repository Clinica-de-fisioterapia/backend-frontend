using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Chronosystem.Infrastructure.Persistence.Repositories;

public sealed class ProfessionalRepository : IProfessionalRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProfessionalRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Professional professional, CancellationToken cancellationToken = default)
        => await _dbContext.Professionals.AddAsync(professional, cancellationToken);

    public async Task<IEnumerable<Professional>> GetAllAsync(Guid? userId, string? registryCode, string? specialty, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Professionals
            .AsNoTracking()
            .Where(p => p.DeletedAt == null);

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(registryCode))
        {
            var normalized = registryCode.Trim();
            query = query.Where(p => p.RegistryCode != null && p.RegistryCode == normalized);
        }

        if (!string.IsNullOrWhiteSpace(specialty))
        {
            var normalized = specialty.Trim();
            query = query.Where(p => p.Specialty != null && EF.Functions.ILike(p.Specialty!, normalized));
        }

        return await query
            .OrderBy(p => p.CreatedAt)
            .ThenBy(p => p.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Professional?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Professionals
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, cancellationToken);

    public async Task<Professional?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Professionals
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, cancellationToken);

    public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _dbContext.Professionals
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.DeletedAt == null, cancellationToken);

    public async Task<bool> RegistryCodeExistsAsync(string registryCode, Guid? excludeProfessionalId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(registryCode))
        {
            return false;
        }

        var normalized = registryCode.Trim();

        return await _dbContext.Professionals
            .AsNoTracking()
            .AnyAsync(p =>
                p.RegistryCode != null &&
                p.RegistryCode == normalized &&
                (!excludeProfessionalId.HasValue || p.Id != excludeProfessionalId.Value) &&
                p.DeletedAt == null,
                cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
