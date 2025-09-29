using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Units.DTOs;
using Mapster; // Adicionado para usar o método .Adapt<>()
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
        // 1. Busca as entidades do banco de dados.
        var units = await _unitRepository.GetAllByTenantAsync();

        // 2. Usa o Mapster para mapear a lista de entidades para uma lista de DTOs.
        return units.Adapt<IEnumerable<UnitDto>>();
    }
}