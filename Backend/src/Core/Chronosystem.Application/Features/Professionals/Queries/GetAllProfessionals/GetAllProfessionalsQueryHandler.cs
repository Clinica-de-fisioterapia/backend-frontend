using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Professionals.DTOs;
using Mapster;
using MediatR;

namespace Chronosystem.Application.Features.Professionals.Queries.GetAllProfessionals;

public sealed class GetAllProfessionalsQueryHandler : IRequestHandler<GetAllProfessionalsQuery, IEnumerable<ProfessionalResponseDto>>
{
    private readonly IProfessionalRepository _professionalRepository;

    public GetAllProfessionalsQueryHandler(IProfessionalRepository professionalRepository)
    {
        _professionalRepository = professionalRepository;
    }

    public async Task<IEnumerable<ProfessionalResponseDto>> Handle(GetAllProfessionalsQuery request, CancellationToken cancellationToken)
    {
        var professionals = await _professionalRepository.GetAllAsync(request.UserId, request.RegistryCode, request.Specialty, cancellationToken);
        return professionals.Adapt<IEnumerable<ProfessionalResponseDto>>();
    }
}
