using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Chronosystem.Infrastructure.Persistence.Repositories;

public class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task AddAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
    }

    public async Task<IEnumerable<User>> GetAllByTenantAsync(Guid tenantId)
    {
        return await _dbContext.Users
            .AsNoTracking() // Melhora a performance em queries de apenas leitura
            .Where(u => u.TenantId == tenantId && u.DeletedAt == null)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid userId, Guid tenantId)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId && u.DeletedAt == null);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null);
    }
    
    public async Task<bool> UserExistsByEmailAsync(string email, Guid tenantId)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Email == email && u.TenantId == tenantId && u.DeletedAt == null);
    }

    public void Remove(User user)
    {
        // Implementação de Soft Delete
        user.DeletedAt = DateTime.UtcNow;
        _dbContext.Users.Update(user);
    }

    public void Update(User user)
    {
        _dbContext.Users.Update(user);
    }

    public Task<int> SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }
}