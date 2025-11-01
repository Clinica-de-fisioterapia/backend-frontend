// ======================================================================================
// ARQUIVO: CreateUnitCommandHandler.cs
// CAMADA: Application / UseCases / Units / Commands / CreateUnit
// OBJETIVO: Executa o caso de uso de criação de uma nova unidade (Unit).
//            Utiliza MediatR para o fluxo CQRS, Mapster para mapeamento DTO
//            e garante auditoria forte a partir do token JWT do ator logado.
// ======================================================================================

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
        // ---------------------------------------------------------------------
        // 1️⃣ Verifica se já existe uma unidade com o mesmo nome
        // ---------------------------------------------------------------------
        bool nameExists = await _unitRepository.UnitNameExistsAsync(request.Name, cancellationToken);
        if (nameExists)
            throw new InvalidOperationException(Messages.Unit_Name_AlreadyExists ?? "Já existe uma unidade com este nome.");

        // ---------------------------------------------------------------------
        // 2️⃣ Cria a entidade de domínio
        // ---------------------------------------------------------------------
        var unit = DomainUnit.Create(request.Name);

        // Auditoria forte: preenchida pelo ator autenticado
        unit.CreatedBy = request.ActorUserId;
        unit.UpdatedBy = request.ActorUserId;

        // ---------------------------------------------------------------------
        // 3️⃣ Persiste no banco
        // ---------------------------------------------------------------------
        await _unitRepository.AddAsync(unit, cancellationToken);
        await _unitRepository.SaveChangesAsync(cancellationToken);

        // ---------------------------------------------------------------------
        // 4️⃣ Mapeia o resultado para o DTO
        // ---------------------------------------------------------------------
        return unit.Adapt<UnitDto>();
    }
}
