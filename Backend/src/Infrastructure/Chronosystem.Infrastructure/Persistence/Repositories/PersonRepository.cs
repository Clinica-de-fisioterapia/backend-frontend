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
            return await _context.People.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Person>> GetAllAsync()
        {
            return await _context.People.ToListAsync();
        }


        // ==========================================================
        //   CPF DUPLICADO — CREATE
        //   Agora aceita string? e evita comparar null x null
        // ==========================================================
        public async Task<bool> ExistsByCpfAsync(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            return await _context.People.AnyAsync(p => p.Cpf == cpf);
        }


        // ==========================================================
        //   CPF DUPLICADO — UPDATE (ignorando o próprio ID)
        // ==========================================================
        public async Task<bool> ExistsByCpfExceptIdAsync(string? cpf, Guid idToIgnore)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            return await _context.People.AnyAsync(p => p.Cpf == cpf && p.Id != idToIgnore);
        }


        // ==========================================================
        //   Métodos não utilizados (se realmente não usar, remova)
        // ==========================================================
        public Task<IEnumerable<Person>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Update(Person person)
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
