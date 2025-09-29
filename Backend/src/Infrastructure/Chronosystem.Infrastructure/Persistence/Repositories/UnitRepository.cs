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

    public async Task<IEnumerable<Unit>> GetAllByTenantAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Units
            .Where(u => u.DeletedAt == null)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Unit?> GetByIdAsync(Guid unitId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && u.DeletedAt == null, cancellationToken);
    }

    public async Task<bool> UnitNameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Units
            .AnyAsync(u => u.Name.ToLower() == name.ToLower() && u.DeletedAt == null, cancellationToken);
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
}
