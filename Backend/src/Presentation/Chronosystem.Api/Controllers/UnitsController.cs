// Chronosystem.Api/Controllers/UnitsController.cs
using Chronosystem.Application.Features.Units.Commands.CreateUnit;
using Chronosystem.Application.Features.Units.Commands.DeleteUnit;
using Chronosystem.Application.Features.Units.Commands.UpdateUnit;
using Chronosystem.Application.Features.Units.DTOs;
// Adicione os usings para as Queries
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chronosystem.Api.Controllers;

[ApiController]
[Route("api/units")]
public class UnitsController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> CreateUnit([FromBody] CreateUnitDto createUnitDto)
    {
        // TODO: Obter TenantId e UserId do contexto do usuário autenticado (JWT)
        var tenantId = Guid.Parse("substituir-pelo-tenant-id-do-token");
        var userId = Guid.Parse("substituir-pelo-user-id-do-token");
        
        var command = new CreateUnitCommand(createUnitDto.Name, tenantId, userId);
        var result = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetUnitById), new { unitId = result.Id }, result);
    }

    [HttpPut("{unitId:guid}")]
    public async Task<IActionResult> UpdateUnit(Guid unitId, [FromBody] UpdateUnitDto updateUnitDto)
    {
        // TODO: Obter TenantId e UserId do contexto do usuário autenticado
        var tenantId = Guid.Parse("substituir-pelo-tenant-id-do-token");
        var userId = Guid.Parse("substituir-pelo-user-id-do-token");

        var command = new UpdateUnitCommand(unitId, updateUnitDto.Name, tenantId, userId);
        await _mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{unitId:guid}")]
    public async Task<IActionResult> DeleteUnit(Guid unitId)
    {
        // TODO: Obter TenantId do contexto do usuário autenticado
        var tenantId = Guid.Parse("substituir-pelo-tenant-id-do-token");

        var command = new DeleteUnitCommand(unitId, tenantId);
        await _mediator.Send(command);
        
        return NoContent();
    }

    // [HttpGet("{unitId:guid}")]
    // public async Task<IActionResult> GetUnitById(Guid unitId) { ... }

    // [HttpGet]
    // public async Task<IActionResult> GetAllUnits() { ... }
}