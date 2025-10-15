// ======================================================================================
// ARQUIVO: UpdateUserCommand.cs
// CAMADA: Application / Features / Users / Commands / UpdateUser
// OBJETIVO: Define o comando CQRS para atualizar um usuário existente.
//           Compatível com o Controller: (id, fullName, role, isActive, updatedBy).
//           Agora suporta atualização opcional de e-mail e senha (caso aplicável).
// ======================================================================================

using Chronosystem.Domain.Entities;
using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommand : IRequest
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public Guid UpdatedBy { get; private set; }

    public UpdateUserCommand(
        Guid id,
        string fullName,
        UserRole role,
        bool isActive,
        Guid updatedBy,
        string? email = null,
        string? password = null)
    {
        Id = id;
        FullName = fullName?.Trim() ?? string.Empty;
        Role = role;
        IsActive = isActive;
        UpdatedBy = updatedBy;
        Email = email?.Trim() ?? string.Empty;
        PasswordHash = password;
    }

    // Construtor padrão opcional (serialização/model binding, se necessário no futuro)
    public UpdateUserCommand() { }
}
