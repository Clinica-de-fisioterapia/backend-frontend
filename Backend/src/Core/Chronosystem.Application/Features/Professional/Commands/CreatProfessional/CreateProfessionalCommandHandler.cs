using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Professionals.Commands.CreateProfessional;
using Chronosystem.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Professionals.Commands.CreateProfessional
{
    public class CreateProfessionalCommandHandler : IRequestHandler<CreateProfessionalCommand, Guid>
    {
        private readonly IProfessionalRepository _professionalRepository;
        private readonly IUnitOfWork _uow;

        public CreateProfessionalCommandHandler(IProfessionalRepository professionalRepository, IUnitOfWork uow)
        {
            _professionalRepository = professionalRepository;
            _uow = uow;
        }

        public async Task<Guid> Handle(CreateProfessionalCommand request, CancellationToken cancellationToken)
        {
            var entity = new Professional
            {
                PersonId = request.Dto.PersonId,
                Specialty = request.Dto.Specialty
            };

            await _professionalRepository.AddAsync(entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
