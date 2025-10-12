using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;

namespace Chronosystem.Application.Features.Units.Commands.UpdateUnit;

public sealed class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand>
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
        var unit = await _unitRepository.GetByIdAsync(request.UnitId, cancellationToken);

        if (unit is null)
        {
            throw new KeyNotFoundException($"Unidade com ID {request.UnitId} não encontrada.");
        }

        unit.UpdateName(request.Name, request.UserId);

        _unitRepository.Update(unit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
