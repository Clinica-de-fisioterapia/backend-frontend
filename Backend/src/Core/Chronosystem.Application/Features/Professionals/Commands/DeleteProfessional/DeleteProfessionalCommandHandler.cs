using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Resources;
using MediatR;

namespace Chronosystem.Application.Features.Professionals.Commands.DeleteProfessional;

public sealed class DeleteProfessionalCommandHandler : IRequestHandler<DeleteProfessionalCommand>
{
    private readonly IProfessionalRepository _professionalRepository;

    public DeleteProfessionalCommandHandler(IProfessionalRepository professionalRepository)
    {
        _professionalRepository = professionalRepository;
    }

    public async Task<Unit> Handle(DeleteProfessionalCommand request, CancellationToken cancellationToken)
    {
        var professional = await _professionalRepository.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (professional is null)
        {
            throw new KeyNotFoundException(Messages.Professional_NotFound);
        }

        professional.SoftDelete(request.ActorUserId);

        await _professionalRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
