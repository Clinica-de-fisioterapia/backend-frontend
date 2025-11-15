// ======================================================================================
// ARQUIVO: CreateUnitCommandHandler.cs
// CAMADA: Application / UseCases / Units / Commands / CreateUnit
// OBJETIVO: Executa o caso de uso de criação de uma nova unidade (Unit).
//            Utiliza MediatR para o fluxo CQRS, Mapster para mapeamento DTO
//            e garante auditoria forte a partir do token JWT do ator logado.
// ======================================================================================

using Chronosystem.Application.Common.Exceptions;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Common.Interfaces.Tenancy;
using Chronosystem.Application.Features.Units.DTOs;
using Chronosystem.Application.Resources;
using DomainUnit = Chronosystem.Domain.Entities.Unit; // ✅ Alias para evitar conflito
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Chronosystem.Application.UseCases.Units.Commands.CreateUnit;

public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, UnitDto>
{
    private readonly IUnitRepository _unitRepository;
    private readonly IPlanQuotaService _planQuotaService;
    private readonly ICurrentTenantProvider _currentTenantProvider;
    private readonly ILogger<CreateUnitCommandHandler> _logger;

    public CreateUnitCommandHandler(
        IUnitRepository unitRepository,
        IPlanQuotaService planQuotaService,
        ICurrentTenantProvider currentTenantProvider,
        ILogger<CreateUnitCommandHandler> logger)
    {
        _unitRepository = unitRepository;
        _planQuotaService = planQuotaService;
        _currentTenantProvider = currentTenantProvider;
        _logger = logger;
    }

    public async Task<UnitDto> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        // ---------------------------------------------------------------------
        // 1️⃣ Verifica se já existe uma unidade com o mesmo nome
        // ---------------------------------------------------------------------
        bool nameExists = await _unitRepository.UnitNameExistsAsync(request.Name, cancellationToken);
        if (nameExists)
            throw new InvalidOperationException(Messages.Unit_Name_AlreadyExists);

        var tenant = _currentTenantProvider.GetCurrentTenant();
        var quotas = _planQuotaService.GetEffectiveQuotas(tenant);

        if (quotas.MaxUnits is int maxUnits && maxUnits >= 0)
        {
            var currentUnits = await _planQuotaService.CountActiveUnitsAsync(tenant, cancellationToken);
            if (currentUnits >= maxUnits)
            {
                _logger.LogWarning(
                    "Plan quota blocked {tenant} max={max} current={current} entity={entity}",
                    tenant,
                    maxUnits,
                    currentUnits,
                    "unit");

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Messages.Plan_MaxUnits_Reached,
                    maxUnits);

                throw new BusinessRuleException(message);
            }
        }

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
