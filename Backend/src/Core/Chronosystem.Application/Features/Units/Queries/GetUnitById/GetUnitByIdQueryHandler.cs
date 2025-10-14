// ======================================================================================
// ARQUIVO: GetUnitByIdQueryHandler.cs
// CAMADA: Application / UseCases / Units / Queries / GetUnitById
// OBJETIVO: Executa a consulta de uma unidade específica pelo seu ID,
//            aplicando o padrão CQRS com MediatR e mapeamento via Mapster.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Units.DTOs;
using Chronosystem.Application.Resources;
using Mapster;
using MediatR;

namespace Chronosystem.Application.UseCases.Units.Queries.GetUnitById;

public class GetUnitByIdQueryHandler : IRequestHandler<GetUnitByIdQuery, UnitDto>
{
    private readonly IUnitRepository _unitRepository;

    public GetUnitByIdQueryHandler(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }

    public async Task<UnitDto> Handle(GetUnitByIdQuery request, CancellationToken cancellationToken)
    {
        // ---------------------------------------------------------------------
        // 1️⃣ Busca a unidade pelo ID (apenas registros não deletados)
        // ---------------------------------------------------------------------
        var unit = await _unitRepository.GetByIdAsync(request.Id, cancellationToken);

        if (unit is null)
            throw new KeyNotFoundException(Messages.Unit_NotFound);

        // ---------------------------------------------------------------------
        // 2️⃣ Mapeia para DTO usando Mapster e retorna
        // ---------------------------------------------------------------------
        return unit.Adapt<UnitDto>();
    }
}
