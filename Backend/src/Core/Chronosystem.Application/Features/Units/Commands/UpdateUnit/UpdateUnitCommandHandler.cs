// ======================================================================================
// ARQUIVO: UpdateUnitCommandHandler.cs
// CAMADA: Application / UseCases / Units / Commands / UpdateUnit
// OBJETIVO: Executa o caso de uso de atualização de uma unidade existente.
//            Aplica CQRS com MediatR, validações multilíngues (.resx),
//            verificação de usuário ativo e mapeamento via Mapster.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Units.DTOs;
using Chronosystem.Application.Resources;
using DomainUnit = Chronosystem.Domain.Entities.Unit; // ✅ Alias para evitar conflito
using Mapster;
using MediatR;

namespace Chronosystem.Application.UseCases.Units.Commands.UpdateUnit;

public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand, UnitDto>
{
    private readonly IUnitRepository _unitRepository;
    private readonly IUserRepository _userRepository;

    public UpdateUnitCommandHandler(
        IUnitRepository unitRepository,
        IUserRepository userRepository)
    {
        _unitRepository = unitRepository;
        _userRepository = userRepository;
    }

    public async Task<UnitDto> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
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
        // 2️⃣ Busca a unidade pelo ID
        // ---------------------------------------------------------------------
        var unit = await _unitRepository.GetByIdAsync(request.Id, cancellationToken);
        if (unit is null)
            throw new KeyNotFoundException(Messages.Unit_NotFound);

        // ---------------------------------------------------------------------
        // 3️⃣ Verifica se já existe outra unidade com o mesmo nome
        // ---------------------------------------------------------------------
        bool nameExists = await _unitRepository.UnitNameExistsAsync(request.Name, cancellationToken);
        if (nameExists && !string.Equals(unit.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(Messages.Unit_Name_AlreadyExists);

        // ---------------------------------------------------------------------
        // 4️⃣ Atualiza o nome e o usuário que modificou
        // ---------------------------------------------------------------------
        unit.UpdateName(request.Name);
        unit.UpdatedBy = request.UserId.Value; // ✅ agora garantido não nulo

        // ---------------------------------------------------------------------
        // 5️⃣ Atualiza o registro no contexto e salva
        // ---------------------------------------------------------------------
        _unitRepository.Update(unit);
        await _unitRepository.SaveChangesAsync(cancellationToken);

        // ---------------------------------------------------------------------
        // 6️⃣ Retorna o resultado mapeado via Mapster
        // ---------------------------------------------------------------------
        return unit.Adapt<UnitDto>();
    }
}
