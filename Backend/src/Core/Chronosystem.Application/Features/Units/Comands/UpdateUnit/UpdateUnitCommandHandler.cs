using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;
using System.Collections.Generic; // Para KeyNotFoundException

namespace Chronosystem.Application.Features.Units.Commands.UpdateUnit;

public class UpdateUnitCommandHandler(IUnitRepository unitRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUnitCommand>
{
    private readonly IUnitRepository _unitRepository = unitRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _unitRepository.GetByIdAsync(request.UnitId);

        if (unit is null)
        {
            throw new KeyNotFoundException($"Unidade com ID {request.UnitId} não encontrada.");
        }

        unit.UpdateName(request.Name);
        unit.UpdatedBy = request.UserId; // O interceptor cuidará do UpdatedAt

        _unitRepository.Update(unit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}