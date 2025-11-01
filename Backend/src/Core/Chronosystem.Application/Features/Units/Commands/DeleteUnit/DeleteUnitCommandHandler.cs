// ======================================================================================
// ARQUIVO: DeleteUnitCommandHandler.cs
// CAMADA: Application / UseCases / Units / Commands
// OBJETIVO: Realiza a exclusão lógica (soft delete) de uma unidade existente.
//            O PostgreSQL definirá automaticamente o campo deleted_at via trigger.
// ======================================================================================

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
        // 1️⃣ Verifica se a unidade existe
        var unit = await _unitRepository.GetByIdAsync(request.Id, cancellationToken);
        if (unit is null)
            throw new InvalidOperationException(Messages.Unit_NotFound);

        // 2️⃣ Define o usuário executor a partir do token
        unit.SoftDelete(request.ActorUserId);

        try
        {
            // 3️⃣ Executa o soft delete (trigger preencherá deleted_at = NOW())
            _unitRepository.Update(unit);
            await _unitRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Caso seja violação de FK ou erro de conexão, normaliza a resposta
            if (ex.Message.Contains("23503") || ex.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(Messages.User_NotFound);

            throw; // Repropaga se for outro tipo de erro
        }

        return MediatR.Unit.Value;
    }
}
