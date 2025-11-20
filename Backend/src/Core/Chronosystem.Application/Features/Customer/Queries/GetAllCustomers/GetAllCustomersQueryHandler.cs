using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Customers.DTOs;
using Chronosystem.Application.Features.People.DTOs;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Customers.Queries.GetAllCustomers
{
    public class GetAllCustomersQueryHandler
        : IRequestHandler<GetAllCustomersQuery, IEnumerable<CustomerDto>>
    {
        private readonly ICustomerRepository _repository;

        public GetAllCustomersQueryHandler(ICustomerRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
        {
            var list = await _repository.GetAllAsync();

            return list.Select(entity => new CustomerDto
            {
                Id = entity.Id,
                PersonId = entity.PersonId,
                Person = new PersonDto
                {
                    Id = entity.Person!.Id,
                    FullName = entity.Person.FullName,
                    Email = entity.Person.Email,
                    Phone = entity.Person.Phone
                }
            });
        }
    }
}
