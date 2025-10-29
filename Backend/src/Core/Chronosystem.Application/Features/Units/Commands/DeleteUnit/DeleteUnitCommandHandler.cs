// ======================================================================================
// ARQUIVO: DeleteUnitCommandHandler.cs
// CAMADA: Application / UseCases / Units / Commands
// OBJETIVO: Realiza a exclusão lógica (soft delete) de uma unidade existente.
//            O PostgreSQL definirá automaticamente updated_at/row_version via trigger.
// ======================================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Resources;
using MediatR;

namespace Chronosystem.Application.UseCases.Units.Commands.DeleteUnit;

public class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand, MediatR.Unit>
{
    private readonly IUnitRepository _unitRepository;

    public DeleteUnitCommandHandler(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }

    public async Task<MediatR.Unit> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
    {
        if (request.ActorUserId == Guid.Empty)
            throw new UnauthorizedAccessException(Messages.Audit_Actor_Required);

        var unit = await _unitRepository.GetByIdAsync(request.Id, cancellationToken);
        if (unit is null)
            throw new InvalidOperationException(Messages.Unit_NotFound);

        unit.SoftDelete(request.ActorUserId);

        await _unitRepository.RemoveAsync(unit, cancellationToken);
        await _unitRepository.SaveChangesAsync(cancellationToken);

        return MediatR.Unit.Value;
    }
}
