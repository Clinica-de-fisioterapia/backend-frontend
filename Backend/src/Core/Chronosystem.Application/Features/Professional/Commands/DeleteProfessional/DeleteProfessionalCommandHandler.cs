using Chronosystem.Application.Common.Interfaces.Persistence;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Professionals.Commands.DeleteProfessional
{
    public class DeleteProfessionalCommandHandler : IRequestHandler<DeleteProfessionalCommand>
    {
        private readonly IProfessionalRepository _repository;
        private readonly IUnitOfWork _uow;

        public DeleteProfessionalCommandHandler(IProfessionalRepository repository, IUnitOfWork uow)
        {
            _repository = repository;
            _uow = uow;
        }

        public async Task<Unit> Handle(DeleteProfessionalCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null)
                throw new ValidationException("Professional not found.");

            // soft-delete: set DeletedAt for audit (ApplicationDbContext SaveChanges interceptor or handler should persist)
            entity.DeletedAt = DateTime.UtcNow;

            await _repository.DeleteAsync(entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
