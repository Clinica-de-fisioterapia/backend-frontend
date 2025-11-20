using Chronosystem.Application.Features.Services.Commands.CreateService;
using Chronosystem.Application.Features.Services.Commands.DeleteService; // ✅ Novo
using Chronosystem.Application.Features.Services.Commands.UpdateService;
using Chronosystem.Application.Features.Services.DTOs;
using Chronosystem.Application.Features.Services.Queries.GetAllServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chronosystem.Api.Controllers;

[ApiController]
[Route("api/services")]
[Authorize]
public class ServicesController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateServiceCommand(dto.Name, dto.DurationMinutes, dto.Price, userId);
        
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceBodyDto body)
    {
        // 1. Pega o ID do usuário logado (do Token JWT)
        var actorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // 2. Monta o comando juntando ID da URL + Corpo do JSON + ID do Usuário
        var command = new UpdateServiceCommand(
            id, 
            body.Name, 
            body.DurationMinutes, 
            body.Price, 
            actorId
        );

        // 3. Envia para o Handler
        await mediator.Send(command);

        // 4. Retorna 204 No Content (padrão para updates que não retornam dados)
        return NoContent();
    }

    // -------------------------------------------------------------------------
    // 🗑️ DELETAR (DELETE)
    // -------------------------------------------------------------------------
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        // 1. Pega o ID do usuário logado
        var actorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // 2. Cria o comando
        var command = new DeleteServiceCommand(id, actorId);

        // 3. Envia para o Handler
        await mediator.Send(command);

        // 4. Retorna 204 No Content
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new GetAllServicesQuery());
        return Ok(result);
    }
}