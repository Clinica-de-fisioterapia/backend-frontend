// ======================================================================================
// ARQUIVO: UnitRepository.cs
// CAMADA: Infrastructure / Persistence / Repositories
// OBJETIVO: Implementa o repositório da entidade Unit, responsável por operações
//            de persistência e leitura no contexto do tenant atual.
//            Compatível com multi-tenant por schema (sem TenantId explícito).
// ======================================================================================

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

public class UnitRepository : IUnitRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UnitRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Unit unit, CancellationToken cancellationToken = default)
    {
        await _dbContext.Units.AddAsync(unit, cancellationToken);
    }

    public void Update(Unit unit)
    {
        _dbContext.Units.Update(unit);
    }

    public Task RemoveAsync(Unit unit, CancellationToken cancellationToken = default)
    {
        _dbContext.Units.Update(unit);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Unit>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Units
            .AsNoTracking()
            .Where(u => u.DeletedAt == null)
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Unit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Units
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, cancellationToken);
    }

    public async Task<bool> UnitNameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Units
            .AsNoTracking()
            .AnyAsync(u => u.Name.ToLower() == name.ToLower() && u.DeletedAt == null, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
