// Chronosystem.Infrastructure/Persistence/Repositories/UnitRepository.cs
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Chronosystem.Infrastructure.Persistence.Repositories;

public class UnitRepository(ApplicationDbContext dbContext) : IUnitRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task AddAsync(Unit unit)
    {
        await _dbContext.Units.AddAsync(unit);
    }

    public async Task<IEnumerable<Unit>> GetAllByTenantAsync(Guid tenantId)
    {
        return await _dbContext.Units
            .Where(u => u.TenantId == tenantId && u.DeletedAt == null)
            .ToListAsync();
    }

    public async Task<Unit?> GetByIdAsync(Guid unitId, Guid tenantId)
    {
        return await _dbContext.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && u.TenantId == tenantId && u.DeletedAt == null);
    }

    public void Remove(Unit unit)
    {
        // Exemplo de Soft Delete
        unit.DeletedAt = DateTime.UtcNow;
        _dbContext.Units.Update(unit);
    }
    
    public void Update(Unit unit)
    {
        _dbContext.Units.Update(unit);
    }

    public Task<int> SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }
}