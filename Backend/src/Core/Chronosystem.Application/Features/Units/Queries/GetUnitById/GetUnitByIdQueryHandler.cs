using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Units.Queries.GetUnitById;

public sealed class GetUnitByIdQueryHandler : IRequestHandler<GetUnitByIdQuery, UnitDto?>
{
    private readonly IUnitRepository _unitRepository;

    public GetUnitByIdQueryHandler(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }

    public async Task<UnitDto?> Handle(GetUnitByIdQuery request, CancellationToken cancellationToken)
    {
        var unit = await _unitRepository.GetByIdAsync(request.UnitId, cancellationToken);

        if (unit is null)
        {
            return null;
        }

        return new UnitDto(unit.Id, unit.Name, unit.CreatedAt, unit.UpdatedAt);
    }
}
