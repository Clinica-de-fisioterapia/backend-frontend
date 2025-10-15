// ======================================================================================
// ARQUIVO: DeleteUserCommand.cs
// CAMADA: Application / Features / Users / Commands / DeleteUser
// OBJETIVO: Define o comando CQRS responsável pela exclusão lógica de usuários.
// ======================================================================================

using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.DeleteUser;

/// <summary>
/// Representa o comando de exclusão lógica de um usuário existente.
/// </summary>
public record DeleteUserCommand(Guid Id) : IRequest;
