using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Infrastructure.Persistence.Repositories
{
    public class ProfessionalRepository : IProfessionalRepository
    {
        private readonly ApplicationDbContext _context;

        public ProfessionalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Professional entity, CancellationToken cancellationToken = default)
        {
            await _context.Professionals.AddAsync(entity, cancellationToken);
        }

        public async Task DeleteAsync(Professional entity, CancellationToken cancellationToken = default)
        {
            _context.Professionals.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task<List<Professional>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Professionals
                .Include(p => p.Person)
                .Where(p => p.DeletedAt == null)
                .ToListAsync(cancellationToken);
        }

        public async Task<Professional?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Professionals
                .Include(p => p.Person)
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, cancellationToken);
        }

        public async Task<bool> ExistsByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default)
        {
            return await _context.Professionals
                .AsNoTracking()
                .AnyAsync(p => p.PersonId == personId && p.DeletedAt == null, cancellationToken);
        }
    }
}
