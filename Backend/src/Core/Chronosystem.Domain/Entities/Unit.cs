using Chronosystem.Domain.Common; // Garanta que o namespace está correto

namespace Chronosystem.Domain.Entities;

public class Unit : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;

    // Construtor privado para o EF Core
    private Unit() { }

 
    public static Unit Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("O nome da unidade não pode ser nulo ou vazio.", nameof(name));
        }

        return new Unit
        {
            Id = Guid.NewGuid(),
            Name = name
        };
    }

    public void UpdateName(string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName))
        {
            Name = newName;
        }
    }
    
    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
    }
}