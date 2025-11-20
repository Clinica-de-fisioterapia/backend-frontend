using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Resources;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.People.Commands.DeletePerson
{
    public class DeletePersonCommandHandler : IRequestHandler<DeletePersonCommand>
    {
        private readonly IPersonRepository _repository;

        public DeletePersonCommandHandler(IPersonRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(DeletePersonCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id);

            if (entity == null)
                throw new Exception(Messages.User_NotFound);

            entity.DeletedAt = DateTime.UtcNow;
            entity.UpdatedBy = request.ActorUserId;

            _repository.Update(entity);
            await _repository.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
