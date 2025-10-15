// ======================================================================================
// ARQUIVO: CreateUserCommand.cs
// CAMADA: Application / Features / Users / Commands / CreateUser
// OBJETIVO: Define o comando CQRS responsável pela criação de usuários.
//           Mantém PasswordHash como propriedade (compatível com validators/handler),
//           mas permite construir a partir de "password" no controller.
// ======================================================================================

using Chronosystem.Domain.Entities;
using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommand : IRequest<Guid>
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    // Mantemos PasswordHash para compatibilidade com o Validator/Handler
    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; } = UserRole.Receptionist;

    // Construtor usado pelo Controller: (fullName, email, password, role)
    public CreateUserCommand(string fullName, string email, string password, UserRole role)
    {
        FullName = fullName?.Trim() ?? string.Empty;
        Email = email?.Trim() ?? string.Empty;

        // TODO: aplicar hashing seguro (ex.: BCrypt) antes de atribuir
        PasswordHash = password ?? string.Empty;

        Role = role;
    }

    // Construtor padrão opcional (serialização/model binding, se necessário no futuro)
    public CreateUserCommand() { }
}
