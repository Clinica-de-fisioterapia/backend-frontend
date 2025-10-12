using System;
using System.Security.Claims;
using Chronosystem.Application.Features.Scheduling.Units.Commands.CreateUnit;
using Chronosystem.Application.Features.Scheduling.Units.Commands.DeleteUnit;
using Chronosystem.Application.Features.Scheduling.Units.Commands.UpdateUnit;
using Chronosystem.Application.Features.Scheduling.Units.DTOs;
using Chronosystem.Application.Features.Scheduling.Units.Queries.GetAllUnits;
using Chronosystem.Application.Features.Scheduling.Units.Queries.GetUnitById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Chronosystem.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/scheduling/units")]
public class UnitsController : ControllerBase
{
    private readonly ISender _mediator;

    public UnitsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Cria uma nova Unidade.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UnitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUnit([FromBody] CreateUnitDto request)
    {
        var userId = GetCurrentUserId();
        var command = new CreateUnitCommand(request.Name, userId);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetUnitById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Busca todas as Unidades do tenant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UnitDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUnits()
    {
        var result = await _mediator.Send(new GetAllUnitsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Busca uma Unidade específica pelo seu ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UnitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUnitById(Guid id)
    {
        var result = await _mediator.Send(new GetUnitByIdQuery(id));

        return result is not null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Atualiza uma Unidade existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitDto dto)
    {
        var userId = GetCurrentUserId();

        await _mediator.Send(new UpdateUnitCommand(id, dto.Name, userId));

        return NoContent();
    }

    /// <summary>
    /// Exclui uma Unidade (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUnit(Guid id)
    {
        var userId = GetCurrentUserId();

        await _mediator.Send(new DeleteUnitCommand(id, userId));
        return NoContent();
    }
    private Guid GetCurrentUserId()
    {
        var subject = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue("sub");

        return Guid.TryParse(subject, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("Usuário não autenticado.");
    }
}