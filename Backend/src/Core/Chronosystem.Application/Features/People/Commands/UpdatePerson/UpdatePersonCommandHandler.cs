using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Resources;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.People.Commands.UpdatePerson
{
    public class UpdatePersonCommandHandler : IRequestHandler<UpdatePersonCommand>
    {
        private readonly IPersonRepository _repository;

        public UpdatePersonCommandHandler(IPersonRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id);

            if (entity == null)
                throw new Exception(Messages.User_NotFound);

            entity.FullName = request.FullName;
            entity.Cpf = request.Cpf;
            entity.Phone = request.Phone;
            entity.Email = request.Email;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = request.ActorUserId;
            entity.RowVersion++;

            _repository.Update(entity);
            await _repository.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
