using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;

namespace Chronosystem.Application.Features.Units.Commands.DeleteUnit;

public sealed class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand>
{
    private readonly IUnitRepository _unitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUnitCommandHandler(IUnitRepository unitRepository, IUnitOfWork unitOfWork)
    {
        _unitRepository = unitRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _unitRepository.GetByIdAsync(request.UnitId, cancellationToken);

        if (unit is null)
        {
            throw new KeyNotFoundException($"Unidade com ID {request.UnitId} não encontrada.");
        }

        unit.SoftDelete(request.UserId);

        _unitRepository.Update(unit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
