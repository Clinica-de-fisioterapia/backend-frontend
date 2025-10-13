// ======================================================================================
// ARQUIVO: UnitsController.cs
// CAMADA: Interface / Controllers
// OBJETIVO: Controlador REST responsável por gerenciar Unidades (Units).
//            Aplica CQRS com MediatR, validações multilíngues e autorização por papéis.
// ======================================================================================

using Chronosystem.Application.Features.Units.DTOs;
using Chronosystem.Application.Resources;
using Chronosystem.Application.UseCases.Units.Commands.CreateUnit;
using Chronosystem.Application.UseCases.Units.Commands.UpdateUnit;
using Chronosystem.Application.UseCases.Units.Commands.DeleteUnit;
using Chronosystem.Application.UseCases.Units.Queries.GetAllUnits;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chronosystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Todos os endpoints exigem usuário autenticado
public class UnitsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UnitsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // -------------------------------------------------------------------------
    // GET /api/units
    // -------------------------------------------------------------------------
    /// <summary>Lista todas as unidades ativas (não deletadas).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllUnitsQuery(), cancellationToken);
        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // POST /api/units
    // -------------------------------------------------------------------------
    /// <summary>Cria uma nova unidade (restrito a administradores).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUnitCommand command, CancellationToken cancellationToken)
    {
        var created = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }

    // -------------------------------------------------------------------------
    // PUT /api/units/{id}
    // -------------------------------------------------------------------------
    /// <summary>Atualiza uma unidade existente.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(Messages.Validation_Id_Mismatch);

        var updated = await _mediator.Send(command, cancellationToken);
        return Ok(updated);
    }

    // -------------------------------------------------------------------------
    // DELETE /api/units/{id}
    // -------------------------------------------------------------------------
    /// <summary>Realiza exclusão lógica (soft delete) de uma unidade.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteUnitCommand(id, userId), cancellationToken);
        return NoContent();
    }
}
