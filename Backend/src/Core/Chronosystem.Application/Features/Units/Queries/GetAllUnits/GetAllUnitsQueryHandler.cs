// ======================================================================================
// ARQUIVO: GetAllUnitsQueryHandler.cs
// CAMADA: Application / UseCases / Units / Queries / GetAllUnits
// OBJETIVO: Executa a leitura de todas as unidades ativas (não deletadas),
//            aplicando o padrão CQRS com MediatR, mapeamento via Mapster e
//            mensagens multilíngues via .resx.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Units.DTOs;
using Chronosystem.Application.Resources;
using Mapster;
using MediatR;

namespace Chronosystem.Application.UseCases.Units.Queries.GetAllUnits;

public class GetAllUnitsQueryHandler : IRequestHandler<GetAllUnitsQuery, IEnumerable<UnitDto>>
{
    private readonly IUnitRepository _unitRepository;

    public GetAllUnitsQueryHandler(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }

    public async Task<IEnumerable<UnitDto>> Handle(GetAllUnitsQuery request, CancellationToken cancellationToken)
    {
        // ---------------------------------------------------------------------
        // 1️⃣ Consulta todas as unidades não deletadas
        // ---------------------------------------------------------------------
        var units = await _unitRepository.GetAllAsync(cancellationToken);

        // ---------------------------------------------------------------------
        // 2️⃣ Verifica se há registros
        // ---------------------------------------------------------------------
        if (!units.Any())
            throw new KeyNotFoundException(Messages.Unit_List_Empty);

        // ---------------------------------------------------------------------
        // 3️⃣ Mapeia o resultado para DTOs via Mapster
        // ---------------------------------------------------------------------
        return units.Adapt<IEnumerable<UnitDto>>();
    }
}
