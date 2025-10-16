// ======================================================================================
// ARQUIVO: CreateUserCommand.cs
// CAMADA: Application / Features / Users / Commands / CreateUser
// OBJETIVO: Define o comando responsável por criar um novo usuário no sistema.
// ======================================================================================

using Chronosystem.Domain.Enums;
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
    UserRole Role
) : IRequest<Guid>;
