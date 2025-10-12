using Chronosystem.Application.Features.Users.Commands.CreateUser;
using Chronosystem.Application.Features.Users.Commands.DeleteUser;
using Chronosystem.Application.Features.Users.Commands.UpdateUser;
using Chronosystem.Application.Features.Users.DTOs;
using Chronosystem.Application.Features.Users.Queries.GetAllUsersByTenant;
using Chronosystem.Application.Features.Users.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // Necessário para ler as claims

namespace Chronosystem.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize] // Protege todos os endpoints por padrão
public class UsersController(ISender mediator) : ControllerBase
{
    // Função helper para extrair claims de forma segura
    private (Guid tenantId, Guid userId) GetUserClaims()
    {
        var tenantId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "tenant_id")!.Value);
        var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
        return (tenantId, userId);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        var (_, createdByUserId) = GetUserClaims();
        var command = new CreateUserCommand(
            createUserDto.FullName,
            createUserDto.Email,
            createUserDto.Password,
            createUserDto.Role,
            createdByUserId);

        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetUserById), new { userId = result.Id }, result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> GetAllUsers()
    {
        var query = new GetAllUsersByTenantQuery();
        var users = await mediator.Send(query);
        return Ok(users);
    }

    [HttpGet("{userId:guid}", Name = "GetUserById")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var query = new GetUserByIdQuery(userId);
        var user = await mediator.Send(query);
        return user is not null ? Ok(user) : NotFound();
    }

    [HttpPut("{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserDto updateUserDto)
    {
        var (_, updatedByUserId) = GetUserClaims();
        var command = new UpdateUserCommand(
            userId,
            updateUserDto.FullName,
            updateUserDto.Role,
            updateUserDto.IsActive,
            updatedByUserId);

        await mediator.Send(command);
        return NoContent(); // Resposta padrão para PUT/UPDATE bem-sucedido
    }

    [HttpDelete("{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var (_, deletedByUserId) = GetUserClaims();
        var command = new DeleteUserCommand(userId, deletedByUserId);
        await mediator.Send(command);
        return NoContent(); // Resposta padrão para DELETE bem-sucedido
    }
}