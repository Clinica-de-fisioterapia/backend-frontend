using Chronosystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chronosystem.Application.Common.Interfaces.Persistence
{
    public interface ICustomerRepository
    {
        Task<Customer> GetByIdAsync(Guid id);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task AddAsync(Customer entity);
        Task UpdateAsync(Customer entity);
        Task DeleteAsync(Customer entity);
    }
}
