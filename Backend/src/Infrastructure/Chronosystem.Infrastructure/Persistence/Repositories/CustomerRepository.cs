using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chronosystem.Infrastructure.Persistence.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Customer> GetByIdAsync(Guid id)
        {
            var customer = await _context.Customers
                .Include(x => x.Person)
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

            if (customer == null)
                throw new KeyNotFoundException($"Customer {id} not found.");

            return customer;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .Include(x => x.Person)
                .Where(x => x.DeletedAt == null)
                .ToListAsync();
        }

        public async Task AddAsync(Customer entity)
        {
            await _context.Customers.AddAsync(entity);
        }

        public Task UpdateAsync(Customer entity)
        {
            _context.Customers.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Customer entity)
        {
            _context.Customers.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
