// ======================================================================================
// ARQUIVO: UpdateUnitCommandHandler.cs
// CAMADA: Application / UseCases / Units / Commands / UpdateUnit
// OBJETIVO: Executa o caso de uso de atualização de uma unidade existente.
//            Aplica CQRS com MediatR, validações multilíngues (.resx)
//            e auditoria forte baseada no ator obtido via JWT.
// ======================================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Units.DTOs;
using Chronosystem.Application.Resources;
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
        if (request.ActorUserId == Guid.Empty)
            throw new UnauthorizedAccessException(Messages.Audit_Actor_Required);

        var unit = await _unitRepository.GetByIdAsync(request.Id, cancellationToken);
        if (unit is null)
            throw new KeyNotFoundException(Messages.Unit_NotFound);

        bool nameExists = await _unitRepository.UnitNameExistsAsync(request.Name, cancellationToken);
        if (nameExists && !string.Equals(unit.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(Messages.Unit_Name_AlreadyExists);

        unit.UpdateName(request.Name);
        unit.UpdatedBy = request.ActorUserId;

        _unitRepository.Update(unit);
        await _unitRepository.SaveChangesAsync(cancellationToken);

        return unit.Adapt<UnitDto>();
    }
}
