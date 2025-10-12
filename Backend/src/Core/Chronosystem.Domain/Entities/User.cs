using Chronosystem.Domain.Common; // Garanta que o namespace está correto

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
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public long RowVersion { get; set; }

    // Construtor privado para o EF Core
    private User() { }

 
    public static User Create(string name, string email, string passwordhash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("O nome da unidade não pode ser nulo ou vazio.", nameof(name));
        }

        if(string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("O e-mail da unidade não pode ser nulo ou vazio.", nameof(email));
        }

        if(string.IsNullOrWhiteSpace(passwordhash))
        {
            throw new ArgumentException("A senha da unidade não pode ser nulo ou vazio.", nameof(passwordhash));
        }

        return new User
        {
            Id = Guid.NewGuid(),
            FullName = name,
            Email = email,
            PasswordHash = passwordhash,
            Role = role
        };
    }

    public void UpdateName(string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName))
        {
            FullName = newName;
        }
    }

    public void UpadateEmail(string newemail)
    {
        if (!string.IsNullOrWhiteSpace(newemail))
        {
            Email = newemail;
        }
    }

    public void UpdatePassword(string newpasswordhash)
    {
        if (!string.IsNullOrWhiteSpace(newpasswordhash))
        {
            PasswordHash = newpasswordhash;
        }
    }

    public void UpdateRole(UserRole newrole)
    {
        Role = newrole;
    }
    
    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
    }
}