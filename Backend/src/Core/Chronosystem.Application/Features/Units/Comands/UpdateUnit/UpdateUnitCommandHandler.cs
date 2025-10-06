using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;
using System.Collections.Generic; // Para KeyNotFoundException

namespace Chronosystem.Application.Features.Units.Commands.UpdateUnit;

// ✅ CORREÇÃO: A interface não tem o segundo parâmetro genérico <Unit>.
public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand>
{
    private readonly IUnitRepository _unitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUnitCommandHandler(IUnitRepository unitRepository, IUnitOfWork unitOfWork)
    {
        _unitRepository = unitRepository;
        _unitOfWork = unitOfWork;
    }

    // ✅ CORREÇÃO: O tipo de retorno é apenas "Task".
    public async Task Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        // ✅ CORREÇÃO: A propriedade no comando é "UnitId".
        var unit = await _unitRepository.GetByIdAsync(request.UnitId);

        if (unit is null)
        {
            throw new KeyNotFoundException($"Unidade com ID {request.UnitId} não encontrada.");
        }

        unit.UpdateName(request.Name);
        unit.UpdatedBy = request.UserId; // Preenche o campo de auditoria

        _unitRepository.Update(unit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ CORREÇÃO: Um método com retorno "Task" não tem "return" de valor no final.
    }
}