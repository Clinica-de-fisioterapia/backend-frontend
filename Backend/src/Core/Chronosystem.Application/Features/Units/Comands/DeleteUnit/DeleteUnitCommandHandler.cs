using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;
namespace Chronosystem.Application.Features.Units.Commands.DeleteUnit;

public class DeleteUnitCommandHandler(IUnitRepository unitRepository) : IRequestHandler<DeleteUnitCommand>
{
    private readonly IUnitRepository _unitRepository = unitRepository;

    public async Task Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _unitRepository.GetByIdAsync(request.UnitId, request.TenantId);
        if (unit is not null)
        {
            _unitRepository.Remove(unit);
            await _unitRepository.SaveChangesAsync();
        }
    }
}