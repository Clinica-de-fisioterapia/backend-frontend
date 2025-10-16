// ======================================================================================
// ARQUIVO: User.cs
// CAMADA: Domain / Entities
// OBJETIVO: Define a entidade de domínio "User", representando um usuário do sistema,
//           com regras de negócio encapsuladas e suporte a auditoria e soft delete.
// ======================================================================================

using Chronosystem.Domain.Common;
using Chronosystem.Domain.Enums;
using System;

namespace Chronosystem.Domain.Entities;

/// <summary>
/// Entidade de domínio que representa um usuário do sistema.
/// </summary>
public class User : AuditableEntity
{
    // -------------------------------------------------------------------------
    // 🧱 PROPRIEDADES
    // -------------------------------------------------------------------------
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>Papel (role) do usuário no sistema.</summary>
    public UserRole Role { get; private set; }

    /// <summary>Indica se o usuário está ativo e pode acessar o sistema.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Controle de concorrência otimista.</summary>
    public long RowVersion { get; private set; }

    protected User() { }

    private User(string name, string email, string passwordHash, UserRole role)
    {
        Id = Guid.NewGuid();
        FullName = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RowVersion = 1;
    }

    public static User Create(string name, string email, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome não pode ser vazio.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("O e-mail não pode ser vazio.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("A senha não pode ser vazia.", nameof(passwordHash));

        return new User(name, email, passwordHash, role);
    }

    // -------------------------------------------------------------------------
    // 🧠 MÉTODOS DE DOMÍNIO
    // -------------------------------------------------------------------------
    public void UpdateName(string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName))
            FullName = newName.Trim();
    }

    public void UpdateEmail(string newEmail)
    {
        if (!string.IsNullOrWhiteSpace(newEmail))
            Email = newEmail.Trim().ToLowerInvariant();
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (!string.IsNullOrWhiteSpace(newPasswordHash))
            PasswordHash = newPasswordHash;
    }

    public void UpdateRole(UserRole newRole) => Role = newRole;

    public void UpdateIsActive(bool active) => IsActive = active;

    public void SoftDelete() => DeletedAt = DateTime.UtcNow;
}
