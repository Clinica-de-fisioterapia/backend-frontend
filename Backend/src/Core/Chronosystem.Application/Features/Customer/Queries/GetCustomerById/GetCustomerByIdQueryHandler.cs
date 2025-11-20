using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Customers.DTOs;
using Chronosystem.Application.Features.People.DTOs;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Customers.Queries.GetCustomerById
{
    public class GetCustomerByIdQueryHandler
        : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
    {
        private readonly ICustomerRepository _repository;

        public GetCustomerByIdQueryHandler(ICustomerRepository repository)
        {
            _repository = repository;
        }

        public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id);

            if (entity == null)
                throw new ValidationException("Customer not found.");
            if (entity.Person == null)
                throw new ValidationException("Person not found for this customer.");
                
            return new CustomerDto
            {
                Id = entity.Id,
                PersonId = entity.PersonId,
                Person = new PersonDto
                {
                    Id = entity.Person.Id,
                    FullName = entity.Person.FullName,
                    Email = entity.Person.Email,
                    Phone = entity.Person.Phone
                }
            };
        }
    }
}
