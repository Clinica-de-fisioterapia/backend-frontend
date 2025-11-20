using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

public class PersonRepository : IPersonRepository
{
    private readonly ApplicationDbContext _context;

    public PersonRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Person person)
    {
        await _context.People.AddAsync(person);
    }

    public Task UpdateAsync(Person person)
    {
        _context.People.Update(person);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Person person)
    {
        _context.People.Remove(person);
        return Task.CompletedTask;
    }

    public async Task<Person?> GetByIdAsync(Guid id)
    {
        return await _context.People
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null);
    }

    public async Task<List<Person>> GetAllAsync()
    {
        return await _context.People
            .Where(p => p.DeletedAt == null)
            .ToListAsync();
    }

    public async Task<bool> ExistsByCpfAsync(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        return await _context.People.AnyAsync(p => p.Cpf == cpf);
    }

    public async Task<bool> ExistsByCpfExceptIdAsync(string? cpf, Guid idToIgnore)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        return await _context.People.AnyAsync(p => p.Cpf == cpf && p.Id != idToIgnore);
    }

    // =============================
    // Métodos obrigatórios da interface
    // =============================

    public async Task<IEnumerable<Person>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.People
            .Where(p => p.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.People
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, cancellationToken);
    }

    public void Update(Person person)
    {
        _context.People.Update(person);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
