using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;
namespace Chronosystem.Application.Features.Units.Commands.UpdateUnit;

public class UpdateUnitCommandHandler(IUnitRepository unitRepository) : IRequestHandler<UpdateUnitCommand>
{
    private readonly IUnitRepository _unitRepository = unitRepository;

    public async Task Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _unitRepository.GetByIdAsync(request.UnitId, request.TenantId);
        if (unit is null) { /* Lançar exceção de não encontrado */ return; }

        unit.Name = request.Name;
        unit.UpdatedBy = request.UserId;
        // O trigger do banco já atualiza UpdatedAt, mas podemos garantir aqui
        unit.UpdatedAt = DateTime.UtcNow;

        _unitRepository.Update(unit);
        await _unitRepository.SaveChangesAsync();
    }
}