using Chronosystem.Application.Common.Interfaces.Persistence;
using FluentValidation;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Customers.Commands.UpdateCustomer
{
    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand>
    {
        private readonly ICustomerRepository _repository;
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _uow;

        public UpdateCustomerCommandHandler(
            ICustomerRepository repository,
            IPersonRepository personRepository,
            IUnitOfWork uow)
        {
            _repository = repository;
            _personRepository = personRepository;
            _uow = uow;
        }

        public async Task<Unit> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id);
            if (entity == null)
                throw new ValidationException("Customer not found.");

            var person = await _personRepository.GetByIdAsync(request.Dto.PersonId);
            if (person == null)
                throw new ValidationException("Person not found.");

            entity.PersonId = request.Dto.PersonId;

            await _repository.UpdateAsync(entity);
            await _uow.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
