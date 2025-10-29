// ======================================================================================
// ARQUIVO: CreateUnitCommandHandler.cs
// CAMADA: Application / UseCases / Units / Commands / CreateUnit
// OBJETIVO: Executa o caso de uso de criação de uma nova unidade (Unit).
//            Utiliza MediatR para o fluxo CQRS, Mapster para mapeamento DTO,
//            FluentValidation para validações e auditoria forte baseada no JWT.
// ======================================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Units.DTOs;
using Chronosystem.Application.Resources;
using DomainUnit = Chronosystem.Domain.Entities.Unit; // ✅ Alias para evitar conflito
using Mapster;
using MediatR;

namespace Chronosystem.Application.UseCases.Units.Commands.CreateUnit;

public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, UnitDto>
{
    private readonly IUnitRepository _unitRepository;

    public CreateUnitCommandHandler(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }

    public async Task<UnitDto> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        if (request.ActorUserId == Guid.Empty)
            throw new UnauthorizedAccessException(Messages.Audit_Actor_Required);

        bool nameExists = await _unitRepository.UnitNameExistsAsync(request.Name, cancellationToken);
        if (nameExists)
            throw new InvalidOperationException(Messages.Unit_Name_AlreadyExists ?? "Já existe uma unidade com este nome.");

        var unit = DomainUnit.Create(request.Name);
        unit.CreatedBy = request.ActorUserId;
        unit.UpdatedBy = request.ActorUserId;

        await _unitRepository.AddAsync(unit, cancellationToken);
        await _unitRepository.SaveChangesAsync(cancellationToken);

        return unit.Adapt<UnitDto>();
    }
}
