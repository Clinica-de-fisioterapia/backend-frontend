using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Units.DTOs;
using Chronosystem.Domain.Entities;
using MediatR;
using Unit = Chronosystem.Domain.Entities.Unit;

namespace Chronosystem.Application.Features.Units.Commands.CreateUnit;

public class CreateUnitCommandHandler(IUnitRepository unitRepository, IUnitOfWork unitOfWork) 
    : IRequestHandler<CreateUnitCommand, UnitDto>
{
    private readonly IUnitRepository _unitRepository = unitRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<UnitDto> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        // Corrigido para chamar Unit.Create sem tenantId
        var unit = Unit.Create(request.Name);
        
        unit.CreatedBy = request.UserId;
        unit.UpdatedBy = request.UserId;

        await _unitRepository.AddAsync(unit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Corrigido para chamar o construtor do DTO sem tenantId
        return new UnitDto(unit.Id, unit.Name, unit.CreatedAt, unit.UpdatedAt);
    }
}