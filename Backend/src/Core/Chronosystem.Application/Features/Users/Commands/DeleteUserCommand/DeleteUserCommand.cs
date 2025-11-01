// ======================================================================================
// ARQUIVO: DeleteUserCommand.cs
// CAMADA: Application / Features / Users / Commands / DeleteUserCommand
// OBJETIVO: Define o comando responsável por realizar o soft delete de um usuário.
// ======================================================================================

using MediatR;
using System;

namespace Chronosystem.Application.Features.Users.Commands.DeleteUserCommand;

/// <summary>
/// Comando CQRS responsável por excluir logicamente (soft delete) um usuário.
/// </summary>
public sealed record DeleteUserCommand(Guid Id) : IRequest
{
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid ActorUserId { get; set; }
}
