// ======================================================================================
// ARQUIVO: UpdateUserCommand.cs
// CAMADA: Application / Features / Users / Commands / UpdateUserCommand
// OBJETIVO: Define o comando responsável por atualizar dados de um usuário existente.
// ======================================================================================

using MediatR;
using System;
using System.Text.Json.Serialization;

namespace Chronosystem.Application.Features.Users.Commands.UpdateUserCommand;

/// <summary>
/// Comando CQRS responsável por atualizar os dados de um usuário existente.
/// O Id vem da rota (PUT /api/users/{id}) e é atribuído no controller.
/// </summary>
public sealed record UpdateUserCommand(
    [property: JsonIgnore] Guid Id,   
    string FullName,
    string Email,
    string? Password,
    string Role,
    bool IsActive
) : IRequest
{
    [JsonIgnore]
    public Guid ActorUserId { get; init; }
}
