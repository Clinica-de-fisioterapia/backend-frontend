using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;

namespace Chronosystem.Application.Features.Units.Commands.DeleteUnit;

public class DeleteUnitCommandHandler(IUnitRepository unitRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteUnitCommand>
{
    private readonly IUnitRepository _unitRepository = unitRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _unitRepository.GetByIdAsync(request.UnitId);

        if (unit is not null)
        {
            _unitRepository.Remove(unit);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    Task<Unit> IRequestHandler<DeleteUnitCommand, Unit>.Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}