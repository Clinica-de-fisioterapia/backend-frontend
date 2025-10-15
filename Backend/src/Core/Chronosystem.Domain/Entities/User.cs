using Chronosystem.Domain.Common;

namespace Chronosystem.Domain.Entities;

public enum UserRole
{
    Admin,
    Professional,
    Receptionist
}

public class User : AuditableEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; set; }
    public long RowVersion { get; set; }

    // ---------------------------------------------------------------------------------
    // EF Core constructor
    // ---------------------------------------------------------------------------------
    private User() { }

    // ---------------------------------------------------------------------------------
    // Factory Method
    // ---------------------------------------------------------------------------------
    public static User Create(string name, string email, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome não pode ser nulo ou vazio.", nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("O e-mail não pode ser nulo ou vazio.", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("A senha não pode ser nula ou vazia.", nameof(passwordHash));

        return new User
        {
            Id = Guid.NewGuid(),
            FullName = name.Trim(),
            Email = email.Trim().ToLower(),
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RowVersion = 1
        };
    }

    // ---------------------------------------------------------------------------------
    // Update Methods (Domain Mutations)
    // ---------------------------------------------------------------------------------
    public void UpdateName(string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName))
            FullName = newName.Trim();
    }

    public void UpdateEmail(string newEmail)
    {
        if (!string.IsNullOrWhiteSpace(newEmail))
            Email = newEmail.Trim().ToLower();
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (!string.IsNullOrWhiteSpace(newPasswordHash))
            PasswordHash = newPasswordHash;
    }

    public void UpdateRole(UserRole newRole)
    {
        Role = newRole;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
    }
}
