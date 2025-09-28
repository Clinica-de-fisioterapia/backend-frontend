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

    // A lógica de tenant agora é controlada pelo schema. Não precisamos mais do tenantId aqui.
    public async Task<IEnumerable<Unit>> GetAllByTenantAsync(Guid tenantId)
    {
        return await _dbContext.Units
            .Where(u => u.DeletedAt == null)
            .ToListAsync();
    }

    // A lógica de tenant também foi removida daqui.
    public async Task<Unit?> GetByIdAsync(Guid unitId, Guid tenantId)
    {
        return await _dbContext.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && u.DeletedAt == null);
    }

    public async Task<bool> UnitNameExistsAsync(string name)
    {
      
        return await _dbContext.Units
            .AnyAsync(u => u.Name.ToLower() == name.ToLower() && u.DeletedAt == null);
    }

    public void Remove(Unit unit)
    {
        unit.SoftDelete();
        _dbContext.Units.Update(unit);
    }
    
    public void Update(Unit unit)
    {
        _dbContext.Units.Update(unit);
    }

    public Task<Unit?> GetByIdAsync(Guid unitId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Unit>> GetAllByTenantAsync()
    {
        throw new NotImplementedException();
    }
}