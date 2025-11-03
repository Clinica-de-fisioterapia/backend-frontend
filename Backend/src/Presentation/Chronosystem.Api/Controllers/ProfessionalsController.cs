using System;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Api.Extensions;
using Chronosystem.Application.Features.Professionals.DTOs;
using Chronosystem.Application.Features.Professionals.Commands.CreateProfessional;
using Chronosystem.Application.Features.Professionals.Commands.DeleteProfessional;
using Chronosystem.Application.Features.Professionals.Commands.UpdateProfessional;
using Chronosystem.Application.Features.Professionals.Queries.GetAllProfessionals;
using Chronosystem.Application.Features.Professionals.Queries.GetProfessionalById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chronosystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public sealed class ProfessionalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfessionalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProfessionalDto dto, CancellationToken cancellationToken)
    {
        if (dto is null)
        {
            return BadRequest("Requisição inválida.");
        }

        var actorId = User.GetActorUserIdOrThrow();
        var command = new CreateProfessionalCommand(dto.UserId, dto.RegistryCode, dto.Specialty)
        {
            ActorUserId = actorId
        };

        var created = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? userId, [FromQuery] string? registryCode, [FromQuery] string? specialty, CancellationToken cancellationToken)
    {
        var query = new GetAllProfessionalsQuery(userId, registryCode, specialty);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var professional = await _mediator.Send(new GetProfessionalByIdQuery(id), cancellationToken);
        return professional is null ? NotFound() : Ok(professional);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProfessionalDto dto, CancellationToken cancellationToken)
    {
        if (dto is null)
        {
            return BadRequest("Requisição inválida.");
        }

        var actorId = User.GetActorUserIdOrThrow();
        var command = new UpdateProfessionalCommand(id, dto.RegistryCode, dto.Specialty)
        {
            ActorUserId = actorId
        };

        var updated = await _mediator.Send(command, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var actorId = User.GetActorUserIdOrThrow();
        var command = new DeleteProfessionalCommand(id)
        {
            ActorUserId = actorId
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
