// ======================================================================================
// ARQUIVO: UsersController.cs
// CAMADA: Interface / Controllers
// OBJETIVO: Controlador REST responsável por gerenciar usuários do sistema.
//            Aplica CQRS (MediatR), autenticação e autorização baseada em papéis.
// ======================================================================================

using Chronosystem.Application.Features.Users.Commands.CreateUser;
using Chronosystem.Application.Features.Users.Commands.DeleteUser;
using Chronosystem.Application.Features.Users.Commands.UpdateUser;
using Chronosystem.Application.Features.Users.DTOs;
using Chronosystem.Application.Features.Users.Queries.GetAllUsers;
using Chronosystem.Application.Features.Users.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chronosystem.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize] // Protege todos os endpoints por padrão
public class UsersController(ISender mediator) : ControllerBase
{
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    // =========================================================================
    // POST: api/users
    // =========================================================================
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        var command = new CreateUserCommand(
            createUserDto.FullName,
            createUserDto.Email,
            createUserDto.Password,
            createUserDto.Role
        );

        var createdUserId = await mediator.Send(command);
        return CreatedAtAction(nameof(GetUserById), new { userId = createdUserId }, createdUserId);
    }

    // =========================================================================
    // GET: api/users
    // =========================================================================
    [HttpGet]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> GetAllUsers()
    {
        var query = new GetAllUsersQuery();
        var users = await mediator.Send(query);
        return Ok(users);
    }

    // =========================================================================
    // GET: api/users/{id}
    // =========================================================================
    [HttpGet("{userId:guid}", Name = "GetUserById")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var query = new GetUserByIdQuery(userId);
        var user = await mediator.Send(query);
        return user is not null ? Ok(user) : NotFound();
    }

    // =========================================================================
    // PUT: api/users/{id}
    // =========================================================================
    [HttpPut("{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserDto updateUserDto)
    {
        var command = new UpdateUserCommand(
            userId,
            updateUserDto.FullName,
            updateUserDto.Role,
            updateUserDto.IsActive,
            GetCurrentUserId()
        );

        await mediator.Send(command);
        return NoContent();
    }

    // =========================================================================
    // DELETE: api/users/{id}
    // =========================================================================
    [HttpDelete("{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var command = new DeleteUserCommand(userId);
        await mediator.Send(command);
        return NoContent();
    }
}
