using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Professionals.DTOs;
using Mapster;
using MediatR;

namespace Chronosystem.Application.Features.Professionals.Queries.GetProfessionalById;

public sealed class GetProfessionalByIdQueryHandler : IRequestHandler<GetProfessionalByIdQuery, ProfessionalResponseDto?>
{
    private readonly IProfessionalRepository _professionalRepository;

    public GetProfessionalByIdQueryHandler(IProfessionalRepository professionalRepository)
    {
        _professionalRepository = professionalRepository;
    }

    public async Task<ProfessionalResponseDto?> Handle(GetProfessionalByIdQuery request, CancellationToken cancellationToken)
    {
        var professional = await _professionalRepository.GetByIdAsync(request.Id, cancellationToken);
        return professional?.Adapt<ProfessionalResponseDto>();
    }
}
