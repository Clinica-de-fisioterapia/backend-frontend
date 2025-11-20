using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Customers.DTOs;
using Chronosystem.Domain.Entities;
using FluentValidation;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler
        : IRequestHandler<CreateCustomerCommand, Guid>
    {
        private readonly ICustomerRepository _repository;
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _uow;

        public CreateCustomerCommandHandler(
            ICustomerRepository repository,
            IPersonRepository personRepository,
            IUnitOfWork uow)
        {
            _repository = repository;
            _personRepository = personRepository;
            _uow = uow;
        }

        public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            // Validate if person exists
            var person = await _personRepository.GetByIdAsync(request.Dto.PersonId);
            if (person == null)
                throw new ValidationException("Person not found.");

            var entity = new Customer
            {
                Id = Guid.NewGuid(),
                PersonId = request.Dto.PersonId
            };

            await _repository.AddAsync(entity);
            await _uow.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
