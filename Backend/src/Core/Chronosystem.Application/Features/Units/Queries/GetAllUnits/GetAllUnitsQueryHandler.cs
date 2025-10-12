using System.Collections.Generic;
using System.Threading;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Units.DTOs;
using Mapster;
using MediatR;

namespace Chronosystem.Application.Features.Units.Queries.GetAllUnits;

public sealed class GetAllUnitsQueryHandler : IRequestHandler<GetAllUnitsQuery, IEnumerable<UnitDto>>
{
    private readonly IUnitRepository _unitRepository;

    public GetAllUnitsQueryHandler(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }

    public async Task<IEnumerable<UnitDto>> Handle(GetAllUnitsQuery request, CancellationToken cancellationToken)
    {
        var units = await _unitRepository.GetAllByTenantAsync(cancellationToken);

        return units.Adapt<IEnumerable<UnitDto>>();
    }
}
