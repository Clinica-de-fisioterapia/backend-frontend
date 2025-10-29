// ======================================================================================
// ARQUIVO: UnitsController.cs
// CAMADA: Interface / Controllers
// OBJETIVO: Controlador REST responsável por gerenciar Unidades (Units).
//            Aplica CQRS com MediatR, validações multilíngues e autorização por papéis.
// ======================================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Features.Units.DTOs;
using Chronosystem.Application.Resources;
using Chronosystem.Application.UseCases.Units.Commands.CreateUnit;
using Chronosystem.Application.UseCases.Units.Commands.UpdateUnit;
using Chronosystem.Application.UseCases.Units.Commands.DeleteUnit;
using Chronosystem.Application.UseCases.Units.Queries.GetAllUnits;
using Chronosystem.Application.UseCases.Units.Queries.GetUnitById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chronosystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnitsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UnitsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] CreateUnitDto dto, CancellationToken cancellationToken)
    {
        if (dto is null)
            return BadRequest(Messages.Validation_Request_Invalid);

        var actorId = GetActorId();
        if (actorId == Guid.Empty)
            return Unauthorized(Messages.Audit_Actor_Required);

        var command = new CreateUnitCommand(dto.Name)
        {
            ActorUserId = actorId
        };

        var created = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllUnitsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUnitByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitDto dto, CancellationToken cancellationToken)
    {
        if (dto is null)
            return BadRequest(Messages.Validation_Request_Invalid);

        if (dto.Id == Guid.Empty)
            return BadRequest(Messages.Unit_Id_Required);

        if (dto.Id != id)
            return BadRequest(Messages.Validation_Id_Mismatch);

        var actorId = GetActorId();
        if (actorId == Guid.Empty)
            return Unauthorized(Messages.Audit_Actor_Required);

        var command = new UpdateUnitCommand(dto.Name)
        {
            Id = id,
            ActorUserId = actorId
        };

        var updated = await _mediator.Send(command, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var actorId = GetActorId();
        if (actorId == Guid.Empty)
            return Unauthorized(Messages.Audit_Actor_Required);

        await _mediator.Send(new DeleteUnitCommand(id)
        {
            ActorUserId = actorId
        }, cancellationToken);

        return NoContent();
    }

    private Guid GetActorId()
        => Guid.TryParse(User.FindFirst("sub")?.Value, out var id) ? id : Guid.Empty;
}
