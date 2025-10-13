// ======================================================================================
// ARQUIVO: CreateUnitCommandHandler.cs
// CAMADA: Application / UseCases / Units / Commands / CreateUnit
// OBJETIVO: Executa o caso de uso de criação de uma nova unidade (Unit).
//            Utiliza MediatR para o fluxo CQRS, Mapster para mapeamento DTO,
//            e FluentValidation (via pipeline) para validações pré-execução.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Units.DTOs;
using Chronosystem.Application.Resources;
using DomainUnit = Chronosystem.Domain.Entities.Unit; // Evita conflito com MediatR.Unit
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
        // 1. Verifica se já existe uma unidade com o mesmo nome
        // ---------------------------------------------------------------------
        bool nameExists = await _unitRepository.UnitNameExistsAsync(request.Name, cancellationToken);
        if (nameExists)
            throw new InvalidOperationException(Messages.Unit_Name_AlreadyExists ?? "Já existe uma unidade com este nome.");

        // ---------------------------------------------------------------------
        // 2. Cria a entidade de domínio (sem persistência ainda)
        // ---------------------------------------------------------------------
        var unit = DomainUnit.Create(request.Name);

        // ---------------------------------------------------------------------
        // 3. Adiciona ao contexto (repositório)
        // ---------------------------------------------------------------------
        await _unitRepository.AddAsync(unit, cancellationToken);
        await _unitRepository.SaveChangesAsync(cancellationToken);

        // ---------------------------------------------------------------------
        // 4. Mapeia o resultado para o DTO com Mapster
        // ---------------------------------------------------------------------
        return unit.Adapt<UnitDto>();
    }
}
