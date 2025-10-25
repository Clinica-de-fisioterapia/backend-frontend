// ======================================================================================
// ARQUIVO: UsersController.cs
// CAMADA: Presentation / Controllers
// OBJETIVO: Controlador responsável por expor endpoints REST relacionados a usuários.
// ======================================================================================

using Chronosystem.Application.Features.Users.Commands.CreateUser;
using Chronosystem.Application.Features.Users.Commands.UpdateUserCommand;
using Chronosystem.Application.Features.Users.Commands.DeleteUserCommand;
using Chronosystem.Application.Features.Users.Queries.GetAllUsers;
using Chronosystem.Application.Features.Users.Queries.GetUserById;
using Chronosystem.Application.Features.Users.DTOs;
using Chronosystem.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Chronosystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // pode ser reativado conforme a política de segurança
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    // -------------------------------------------------------------------------
    // ➕ POST /api/users
    // -------------------------------------------------------------------------
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (dto is null)
            return BadRequest(Messages.Validation_Request_Invalid);

        // Conversão segura DTO -> Command
        var command = new CreateUserCommand(
            dto.FullName,
            dto.Email,
            dto.Password,
            dto.Role
        );

        var userId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = userId }, new { id = userId });
    }

    // -------------------------------------------------------------------------
    // 🔍 GET /api/users
    // -------------------------------------------------------------------------
    [HttpGet]
    [Authorize(Roles = "admin,receptionist")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllUsersQuery());
        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // 🔍 GET /api/users/{id}
    // -------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "admin,receptionist")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id));
        return user is null ? NotFound(Messages.User_NotFound) : Ok(user);
    }


    // -------------------------------------------------------------------------
    // ✏️ PUT /api/users/{id}
    // -------------------------------------------------------------------------
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand command)
    {
        // If the body ever includes Id (future change), validate mismatch.
        if (command.Id != Guid.Empty && command.Id != id)
            return BadRequest(Messages.Validation_Id_Mismatch);

        // Id comes from route; fix the command with the path Id
        var fixedCommand = command with { Id = id };

        await _mediator.Send(fixedCommand);
        return NoContent();
    }


    // -------------------------------------------------------------------------
    // ❌ DELETE /api/users/{id}
    // -------------------------------------------------------------------------
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteUserCommand(id));
        return NoContent();
    }

    // -------------------------------------------------------------------------
    // ⚙️ GET /api/users/roles
    // -------------------------------------------------------------------------
    [HttpGet("roles")]
    [Authorize(Roles = "admin,receptionist")]
    public async Task<IActionResult> GetRoles()
    {
        var users = await _mediator.Send(new GetAllUsersQuery());
        var roles = users
            .Select(u => u.Role)
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(role => role, StringComparer.OrdinalIgnoreCase);

        return Ok(new { roles });
    }
}
