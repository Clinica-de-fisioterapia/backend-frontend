// ======================================================================================
// ARQUIVO: CreateUnitCommandHandler.cs
// CAMADA: Application / UseCases / Units / Commands / CreateUnit
// OBJETIVO: Executa o caso de uso de criação de uma nova unidade (Unit).
//            Utiliza MediatR para o fluxo CQRS, Mapster para mapeamento DTO,
//            FluentValidation para validações e checagem de usuário ativo.
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
    private readonly IUserRepository _userRepository;

    public CreateUnitCommandHandler(
        IUnitRepository unitRepository,
        IUserRepository userRepository)
    {
        _unitRepository = unitRepository;
        _userRepository = userRepository;
    }

    public async Task<UnitDto> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        // ---------------------------------------------------------------------
        // 1️⃣ Validação de usuário responsável
        // ---------------------------------------------------------------------
        if (!request.UserId.HasValue || request.UserId == Guid.Empty)
            throw new InvalidOperationException(Messages.Validation_UserId_Required);

        bool userExists = await _userRepository.ExistsAndIsActiveAsync(request.UserId.Value, cancellationToken);
        if (!userExists)
            throw new InvalidOperationException(Messages.User_NotFound ?? "Usuário responsável não encontrado ou inativo.");

        // ---------------------------------------------------------------------
        // 2️⃣ Verifica se já existe uma unidade com o mesmo nome
        // ---------------------------------------------------------------------
        bool nameExists = await _unitRepository.UnitNameExistsAsync(request.Name, cancellationToken);
        if (nameExists)
            throw new InvalidOperationException(Messages.Unit_Name_AlreadyExists ?? "Já existe uma unidade com este nome.");

        // ---------------------------------------------------------------------
        // 3️⃣ Cria a entidade de domínio
        // ---------------------------------------------------------------------
        var unit = DomainUnit.Create(request.Name);

        // Preenche o campo created_by com o usuário válido
        unit.CreatedBy = request.UserId.Value;

        // ---------------------------------------------------------------------
        // 4️⃣ Persiste no banco
        // ---------------------------------------------------------------------
        await _unitRepository.AddAsync(unit, cancellationToken);
        await _unitRepository.SaveChangesAsync(cancellationToken);

        // ---------------------------------------------------------------------
        // 5️⃣ Mapeia o resultado para o DTO
        // ---------------------------------------------------------------------
        return unit.Adapt<UnitDto>();
    }
}
