using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Professionals.DTOs;
using Chronosystem.Application.Resources;
using Mapster;
using MediatR;

namespace Chronosystem.Application.Features.Professionals.Commands.UpdateProfessional;

public sealed class UpdateProfessionalCommandHandler : IRequestHandler<UpdateProfessionalCommand, ProfessionalResponseDto>
{
    private readonly IProfessionalRepository _professionalRepository;

    public UpdateProfessionalCommandHandler(IProfessionalRepository professionalRepository)
    {
        _professionalRepository = professionalRepository;
    }

    public async Task<ProfessionalResponseDto> Handle(UpdateProfessionalCommand request, CancellationToken cancellationToken)
    {
        var professional = await _professionalRepository.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (professional is null)
        {
            throw new KeyNotFoundException(Messages.Professional_NotFound);
        }

        if (!string.IsNullOrWhiteSpace(request.RegistryCode))
        {
            var normalizedRegistryCode = request.RegistryCode.Trim();
            var registryExists = await _professionalRepository.RegistryCodeExistsAsync(normalizedRegistryCode, professional.Id, cancellationToken);
            if (registryExists)
            {
                throw new InvalidOperationException(Messages.Professional_RegistryCode_AlreadyExists);
            }
        }

        professional.UpdateRegistryCode(request.RegistryCode);
        professional.UpdateSpecialty(request.Specialty);
        professional.UpdatedBy = request.ActorUserId;

        await _professionalRepository.SaveChangesAsync(cancellationToken);

        return professional.Adapt<ProfessionalResponseDto>();
    }
}
