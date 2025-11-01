// ======================================================================================
// ARQUIVO: CreateUserCommand.cs
// CAMADA: Application / Features / Users / Commands / CreateUser
// OBJETIVO: Define o comando responsável por criar um novo usuário no sistema.
// ======================================================================================

using MediatR;
using System;

namespace Chronosystem.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Comando CQRS responsável por criar um novo usuário no sistema.
/// </summary>
public sealed record CreateUserCommand(
    string FullName,
    string Email,
    string Password,
    string Role
) : IRequest<Guid>
{
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid ActorUserId { get; set; }
}
