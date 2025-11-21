using Chronosystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Common.Interfaces.Persistence
{
    public interface IPersonRepository
    {
        Task AddAsync(Person person);
        Task<IEnumerable<Person>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        void Update(Person person);
        Task<bool> ExistsByEmailAsync(string? email);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsByCpfAsync(string? cpf);
        Task<bool> ExistsByCpfExceptIdAsync(string cpf, Guid idToIgnore);
        Task<bool> ExistsByEmailExceptIdAsync(string? email, Guid idToIgnore);

    Task UpdateAsync(Person person);
  }
} 
