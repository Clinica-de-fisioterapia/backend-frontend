// ======================================================================================
// ARQUIVO: User.cs
// CAMADA: Domain / Entities
// OBJETIVO: Define a entidade de domínio "User", representando um usuário do sistema,
//           com regras de negócio encapsuladas e suporte a auditoria e soft delete.
// ======================================================================================

using Chronosystem.Domain.Common;
using System;

namespace Chronosystem.Domain.Entities;

/// <summary>
/// Entidade de domínio que representa um usuário do sistema.
/// </summary>
public class User : AuditableEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty; // CITEXT no banco
    public string PasswordHash { get; private set; } = string.Empty;
    // Papel global flexível (TEXT no banco). Nunca vincular a enum fixa.
    public string Role { get; private set; } = "receptionist";
    public bool IsActive { get; private set; } = true;
    public long RowVersion { get; private set; }

    private User() { }

    public static User Create(string fullName, string email, string passwordHash, string role)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name is required.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        if (string.IsNullOrWhiteSpace(role)) throw new ArgumentException("Role is required.", nameof(role));

        return new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            // normaliza para minúsculas; regex é validada na camada de aplicação
            Role = role.Trim().ToLowerInvariant(),
            IsActive = true
        };
    }

    public void UpdateName(string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName))
        {
            FullName = newName.Trim();
        }
    }

    public void UpdateEmail(string newEmail)
    {
        if (!string.IsNullOrWhiteSpace(newEmail))
        {
            Email = newEmail.Trim().ToLowerInvariant();
        }
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (!string.IsNullOrWhiteSpace(newPasswordHash))
        {
            PasswordHash = newPasswordHash;
        }
    }

    public void UpdateRole(string newRole)
    {
        if (!string.IsNullOrWhiteSpace(newRole))
        {
            Role = newRole.Trim().ToLowerInvariant();
        }
    }

    public void UpdateIsActive(bool active) => IsActive = active;

    public void SoftDelete() => DeletedAt = DateTime.UtcNow;
}
