// Features/Units/Commands/CreateUnit/CreateUnitCommandHandler.cs
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Units.DTOs;
using Chronosystem.Domain.Entities;
using MediatR;

namespace Chronosystem.Application.Features.Units.Commands.CreateUnit;

public class CreateUnitCommandHandler(IUnitRepository unitRepository) : IRequestHandler<CreateUnitCommand, UnitDto>
{
    private readonly IUnitRepository _unitRepository = unitRepository;

    public async Task<UnitDto> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = new Unit
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Name = request.Name,
            CreatedBy = request.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = request.UserId
        };

        await _unitRepository.AddAsync(unit);
        await _unitRepository.SaveChangesAsync();

        return new UnitDto(unit.Id, unit.TenantId, unit.Name, unit.CreatedAt, unit.UpdatedAt);
    }
}