// ======================================================================================
// ARQUIVO: UnitsController.cs
// CAMADA: Interface / Controllers
// OBJETIVO: Controlador REST responsável por gerenciar Unidades (Units).
//            Aplica CQRS com MediatR, validações multilíngues e autorização por papéis.
// ======================================================================================

using Chronosystem.Api.Extensions;
using Chronosystem.Application.Features.Units.DTOs;
using Chronosystem.Application.UseCases.Units.Commands.CreateUnit;
using Chronosystem.Application.UseCases.Units.Commands.DeleteUnit;
using Chronosystem.Application.UseCases.Units.Commands.UpdateUnit;
using Chronosystem.Application.UseCases.Units.Queries.GetAllUnits;
using Chronosystem.Application.UseCases.Units.Queries.GetUnitById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chronosystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UnitsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UnitsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // -------------------------------------------------------------------------
    // POST /api/units
    // -------------------------------------------------------------------------
    /// <summary>Cria uma nova unidade (restrito a administradores).</summary>
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] CreateUnitDto dto, CancellationToken cancellationToken)
    {
        if (dto is null)
            return BadRequest("Requisição inválida.");

        var actorId = User.GetActorUserIdOrThrow();

        var command = new CreateUnitCommand(dto.Name)
        {
            ActorUserId = actorId
        };

        var created = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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
    // GET /api/units/{id}
    // -------------------------------------------------------------------------
    /// <summary>Obtém os detalhes de uma unidade específica pelo seu ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUnitByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    // -------------------------------------------------------------------------
    // PUT /api/units/{id}
    // -------------------------------------------------------------------------
    /// <summary>Atualiza uma unidade existente.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitDto dto, CancellationToken cancellationToken)
    {
        if (dto is null)
            return BadRequest("Requisição inválida.");

        var actorId = User.GetActorUserIdOrThrow();

        var command = new UpdateUnitCommand(dto.Name)
        {
            Id = id,
            ActorUserId = actorId
        };

        var updated = await _mediator.Send(command, cancellationToken);
        return Ok(updated);
    }


    // -------------------------------------------------------------------------
    // DELETE /api/units/{id}
    // -------------------------------------------------------------------------
    /// <summary>Realiza exclusão lógica (soft delete) de uma unidade.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var actorId = User.GetActorUserIdOrThrow();

        var command = new DeleteUnitCommand(id)
        {
            ActorUserId = actorId
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
