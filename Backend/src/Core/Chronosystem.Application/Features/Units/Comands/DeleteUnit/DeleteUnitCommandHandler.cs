using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Resources;
using DomainUnit = Chronosystem.Domain.Entities.Unit;
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
        var unit = await _unitRepository.GetByIdAsync(request.Id, cancellationToken);
        if (unit is null)
            throw new InvalidOperationException(Messages.Unit_NotFound);

        _unitRepository.Remove(unit);
        await _unitRepository.SaveChangesAsync(cancellationToken);

        return MediatR.Unit.Value;
    }
}
