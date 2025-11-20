using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Resources;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.People.Commands.UpdatePerson
{
    public class UpdatePersonCommandHandler : IRequestHandler<UpdatePersonCommand>
    {
        private readonly IPersonRepository _repository;
        private readonly IUnitOfWork _uow;

        public UpdatePersonCommandHandler(IPersonRepository repository, IUnitOfWork uow)
        {
            _repository = repository;
            _uow = uow;
        }

        public async Task<Unit> Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
        {
            var person = await _repository.GetByIdAsync(request.Id);

            if (person is null)
                throw new ValidationException("Person not found.");

            // Atualização simples
            person.FullName = request.FullName;
            person.Email = request.Email;
            person.Phone = request.Phone;
            person.Cpf = request.Cpf;

            _repository.Update(person);
            await _uow.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
