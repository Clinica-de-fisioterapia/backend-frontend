// ======================================================================================
// ARQUIVO: UpdateUserCommand.cs
// CAMADA: Application / Features / Users / Commands / UpdateUserCommand
// OBJETIVO: Define o comando responsável por atualizar dados de um usuário existente.
// ======================================================================================

using MediatR;
using System;

namespace Chronosystem.Application.Features.Users.Commands.UpdateUserCommand;

/// <summary>
/// Comando CQRS responsável por atualizar os dados de um usuário existente.
/// </summary>
public sealed record UpdateUserCommand(
    Guid Id,
    string FullName,
    string Email,
    string? Password,
    string Role,
    bool IsActive
) : IRequest;
