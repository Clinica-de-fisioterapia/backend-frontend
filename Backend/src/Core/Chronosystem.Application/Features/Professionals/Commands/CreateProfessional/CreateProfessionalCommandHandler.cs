using System;
using System.Collections.Generic;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Professionals.DTOs;
using Chronosystem.Application.Resources;
using Chronosystem.Domain.Entities;
using Mapster;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Professionals.Commands.CreateProfessional;

public sealed class CreateProfessionalCommandHandler : IRequestHandler<CreateProfessionalCommand, ProfessionalResponseDto>
{
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IUserRepository _userRepository;

    public CreateProfessionalCommandHandler(
        IProfessionalRepository professionalRepository,
        IUserRepository userRepository)
    {
        _professionalRepository = professionalRepository;
        _userRepository = userRepository;
    }

    public async Task<ProfessionalResponseDto> Handle(CreateProfessionalCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new KeyNotFoundException(Messages.Professional_User_NotFound);
        }

        if (!string.Equals(user.Role, "professional", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(Messages.Professional_User_InvalidRole);
        }

        var professionalExistsForUser = await _professionalRepository.ExistsByUserIdAsync(request.UserId, cancellationToken);
        if (professionalExistsForUser)
        {
            throw new InvalidOperationException(Messages.Professional_User_AlreadyLinked);
        }

        if (!string.IsNullOrWhiteSpace(request.RegistryCode))
        {
            var normalizedRegistryCode = request.RegistryCode.Trim();
            var registryCodeExists = await _professionalRepository.RegistryCodeExistsAsync(normalizedRegistryCode, null, cancellationToken);
            if (registryCodeExists)
            {
                throw new InvalidOperationException(Messages.Professional_RegistryCode_AlreadyExists);
            }
        }

        var professional = Professional.Create(request.UserId, request.RegistryCode, request.Specialty);
        professional.CreatedBy = request.ActorUserId;
        professional.UpdatedBy = request.ActorUserId;

        await _professionalRepository.AddAsync(professional, cancellationToken);
        await _professionalRepository.SaveChangesAsync(cancellationToken);

        return professional.Adapt<ProfessionalResponseDto>();
    }
}
