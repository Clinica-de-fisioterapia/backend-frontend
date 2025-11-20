using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.People.Commands.CreatePerson
{
    public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, Guid>
    {
        private readonly IPersonRepository _repository;

        public CreatePersonCommandHandler(IPersonRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Cpf = request.Cpf,
                Phone = request.Phone,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RowVersion = 1,
                CreatedBy = request.ActorUserId
            };

            await _repository.AddAsync(person);
            await _repository.SaveChangesAsync();

            return person.Id;
        }
    }
}
