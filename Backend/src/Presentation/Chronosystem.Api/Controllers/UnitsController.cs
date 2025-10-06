using Chronosystem.Application.Features.Units.Commands.CreateUnit;
using Chronosystem.Application.Features.Units.Queries.GetAllUnits;
// TODO: Adicionar os usings para os outros Commands e Queries quando forem criados
// using Chronosystem.Application.Features.Units.Commands.UpdateUnit;
// using Chronosystem.Application.Features.Units.Commands.DeleteUnit;
// using Chronosystem.Application.Features.Units.Queries.GetUnitById;
using Chronosystem.Application.Features.Units.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Chronosystem.Application.Features.Units.Commands.UpdateUnit;
using Chronosystem.Application.Features.Units.Commands.DeleteUnit;

namespace Chronosystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // Rota base: /api/units
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
    public async Task<IActionResult> CreateUnit([FromBody] CreateUnitCommand command)
    {
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
        // Esta chamada agora executa a busca real no banco de dados.
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
        // TODO: Implementar GetUnitByIdQuery e GetUnitByIdQueryHandler.
        // var query = new GetUnitByIdQuery(id);
        // var result = await _mediator.Send(query);
        // return result is not null ? Ok(result) : NotFound();

        await Task.CompletedTask;
        return Ok(new UnitDto(id, "Unidade de Teste (GET por ID)", DateTime.UtcNow, DateTime.UtcNow));
    }

    /// <summary>
    /// Atualiza uma Unidade existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUnit(Guid id, [FromBody] UpdateUnitDto dto)
    {
        await _mediator.Send(new UpdateUnitCommand(id, dto.Name));
        return NoContent();
    }


    /// <summary>
    /// Exclui uma Unidade (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUnit(Guid id)
    {
        // Cria o comando com o ID vindo da rota.
        var command = new DeleteUnitCommand(id);
        
        // Envia o comando para o handler correspondente.
        await _mediator.Send(command);

        // Retorna 204 No Content, indicando sucesso na exclusão.
        return NoContent();
    }
}