// ======================================================================================
// ARQUIVO: UpdateUnitCommandHandler.cs
// CAMADA: Application / UseCases / Units / Commands / UpdateUnit
// OBJETIVO: Executa o caso de uso de atualização de uma unidade existente.
//            Aplica CQRS com MediatR, validações multilíngues (.resx),
//            e mapeamento de saída via Mapster.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Units.DTOs;
using Chronosystem.Application.Resources;
using Chronosystem.Domain.Entities;
using Mapster;
using MediatR;

namespace Chronosystem.Application.UseCases.Units.Commands.UpdateUnit;

public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand, UnitDto>
{
    private readonly IUnitRepository _unitRepository;

    public UpdateUnitCommandHandler(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }

    public async Task<UnitDto> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        // ---------------------------------------------------------------------
        // 1️⃣ Busca a unidade pelo ID
        // ---------------------------------------------------------------------
        var unit = await _unitRepository.GetByIdAsync(request.Id, cancellationToken);

        if (unit is null)
            throw new KeyNotFoundException(Messages.Unit_NotFound);

        // ---------------------------------------------------------------------
        // 2️⃣ Verifica se já existe outra unidade com o mesmo nome
        // ---------------------------------------------------------------------
        bool nameExists = await _unitRepository.UnitNameExistsAsync(request.Name, cancellationToken);
        if (nameExists && !string.Equals(unit.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(Messages.Unit_Name_AlreadyExists);

        // ---------------------------------------------------------------------
        // 3️⃣ Atualiza o nome (aplica regra de domínio)
        // ---------------------------------------------------------------------
        unit.UpdateName(request.Name);

        // ---------------------------------------------------------------------
        // 4️⃣ Atualiza o registro no contexto e salva
        // ---------------------------------------------------------------------
        _unitRepository.Update(unit);
        await _unitRepository.SaveChangesAsync(cancellationToken);

        // ---------------------------------------------------------------------
        // 5️⃣ Retorna o resultado mapeado via Mapster
        // ---------------------------------------------------------------------
        return unit.Adapt<UnitDto>();
    }
}
