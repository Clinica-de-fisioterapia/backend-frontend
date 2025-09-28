using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Units.Queries.GetAllUnits;

public class GetAllUnitsQueryHandler : IRequestHandler<GetAllUnitsQuery, IEnumerable<UnitDto>>
{
    private readonly IUnitRepository _unitRepository;

    public GetAllUnitsQueryHandler(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }

    public async Task<IEnumerable<UnitDto>> Handle(GetAllUnitsQuery request, CancellationToken cancellationToken)
    {
        var units = await _unitRepository.GetAllByTenantAsync();

        return units.Select(unit => new UnitDto(
            unit.Id,
            unit.Name,
            unit.CreatedAt,
            unit.UpdatedAt
        )).ToList();
    }
}