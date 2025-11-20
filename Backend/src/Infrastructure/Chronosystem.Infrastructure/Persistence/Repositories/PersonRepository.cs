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
    public class PersonRepository : IPersonRepository
    {
        private readonly ApplicationDbContext _db;

        public PersonRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Person person)
            => await _db.People.AddAsync(person);

        public async Task<IEnumerable<Person>> GetAllAsync(CancellationToken ct = default)
            => await _db.People
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .OrderBy(x => x.FullName)
                .ToListAsync(ct);

        public async Task<Person?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _db.People
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, ct);

        public void Update(Person entity)
            => _db.People.Update(entity);

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
