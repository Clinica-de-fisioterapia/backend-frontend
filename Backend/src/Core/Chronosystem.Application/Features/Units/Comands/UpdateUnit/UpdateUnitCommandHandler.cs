using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;
using System.Collections.Generic;

namespace Chronosystem.Application.Features.Units.Commands.UpdateUnit;

public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand, Unit>
{
    private readonly IUnitRepository _unitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUnitCommandHandler(IUnitRepository unitRepository, IUnitOfWork unitOfWork)
    {
        _unitRepository = unitRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _unitRepository.GetByIdAsync(request.UnitId);

        if (unit is null)
            throw new KeyNotFoundException($"Unidade com ID {request.UnitId} não encontrada.");

        unit.UpdateName(request.Name);
        unit.UpdatedBy = request.UserId;

        _unitRepository.Update(unit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
