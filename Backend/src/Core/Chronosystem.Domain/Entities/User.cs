using Chronosystem.Domain.Common;

namespace Chronosystem.Domain.Entities;

public enum UserRole
{
    Admin,
    Professional,
    Receptionist
}

public sealed class User : AuditableEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public long RowVersion { get; private set; }

    private User()
    {
    }

    private User(Guid id, string fullName, string email, string passwordHash, UserRole role, Guid createdBy)
    {
        Id = id;
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        RowVersion = 1;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
    }

    public static User Create(string name, string email, string passwordHash, UserRole role, Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("O nome do usuário não pode ser nulo ou vazio.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("O e-mail do usuário não pode ser nulo ou vazio.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("O hash da senha não pode ser nulo ou vazio.", nameof(passwordHash));
        }

        var normalizedName = name.Trim();
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return new User(Guid.NewGuid(), normalizedName, normalizedEmail, passwordHash, role, createdBy);
    }

    public void UpdateName(string newName, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("O nome do usuário não pode ser nulo ou vazio.", nameof(newName));
        }

        var normalized = newName.Trim();
        if (string.Equals(FullName, normalized, StringComparison.Ordinal))
        {
            return;
        }

        FullName = normalized;
        Touch(updatedBy);
    }

    public void UpdateEmail(string newEmail, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
        {
            throw new ArgumentException("O e-mail do usuário não pode ser nulo ou vazio.", nameof(newEmail));
        }

        var normalized = newEmail.Trim().ToLowerInvariant();
        if (string.Equals(Email, normalized, StringComparison.Ordinal))
        {
            return;
        }

        Email = normalized;
        Touch(updatedBy);
    }

    public void UpdatePassword(string newPasswordHash, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new ArgumentException("O hash da senha não pode ser nulo ou vazio.", nameof(newPasswordHash));
        }

        PasswordHash = newPasswordHash;
        Touch(updatedBy);
    }

    public void UpdateRole(UserRole newRole, Guid updatedBy)
    {
        if (Role == newRole)
        {
            return;
        }

        Role = newRole;
        Touch(updatedBy);
    }

    public void SetActiveStatus(bool isActive, Guid updatedBy)
    {
        if (IsActive == isActive)
        {
            return;
        }

        IsActive = isActive;
        Touch(updatedBy);
    }

    public void SoftDelete(Guid deletedBy)
    {
        if (DeletedAt.HasValue)
        {
            return;
        }

        DeletedAt = DateTime.UtcNow;
        Touch(deletedBy);
        UpdatedAt = DeletedAt.Value;
    }

    private void Touch(Guid updatedBy)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        RowVersion++;
    }
}
