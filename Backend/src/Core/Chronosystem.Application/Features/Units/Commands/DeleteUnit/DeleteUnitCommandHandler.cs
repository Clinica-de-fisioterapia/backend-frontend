// ======================================================================================
// ARQUIVO: DeleteUnitCommandHandler.cs
// CAMADA: Application / UseCases / Units / Commands
// OBJETIVO: Realiza a exclusão lógica (soft delete) de uma unidade existente.
//            O PostgreSQL definirá automaticamente o campo deleted_at via trigger.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Resources;
using DomainUnit = Chronosystem.Domain.Entities.Unit;
using MediatR;

namespace Chronosystem.Application.UseCases.Units.Commands.DeleteUnit;

public class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand, MediatR.Unit>
{
    private readonly IUnitRepository _unitRepository;
    private readonly IUserRepository _userRepository;

    public DeleteUnitCommandHandler(IUnitRepository unitRepository, IUserRepository userRepository)
    {
        _unitRepository = unitRepository;
        _userRepository = userRepository;
    }

    public async Task<MediatR.Unit> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ Verifica se a unidade existe
        var unit = await _unitRepository.GetByIdAsync(request.Id, cancellationToken);
        if (unit is null)
            throw new InvalidOperationException(Messages.Unit_NotFound);

        // 2️⃣ Valida o ID do usuário
        if (request.UserId == Guid.Empty)
            throw new InvalidOperationException(Messages.Validation_UserId_Required);

        // 3️⃣ Garante que o usuário exista (evita FK violation)
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            throw new InvalidOperationException(Messages.User_NotFound);

        // 4️⃣ Define o usuário executor
        unit.UpdatedBy = request.UserId;

        try
        {
            // 5️⃣ Executa o soft delete (trigger preencherá deleted_at = NOW())
            await _unitRepository.RemoveAsync(unit, cancellationToken);
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
