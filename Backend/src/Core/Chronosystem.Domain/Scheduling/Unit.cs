using System;
using Chronosystem.Domain.Common;

namespace Chronosystem.Domain.Scheduling;

/// <summary>
/// Representa uma unidade física do tenant dentro do domínio de agendamento.
/// As unidades são isoladas por schema (multi-tenant) e são auditadas.
/// </summary>
public sealed class Unit : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;

    private Unit()
    {
    }

    private Unit(Guid id, string name, Guid createdBy)
    {
        Id = id;
        Name = name;
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public static Unit Create(string name, Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("O nome da unidade não pode ser nulo ou vazio.", nameof(name));
        }

        return new Unit(Guid.NewGuid(), name.Trim(), createdBy);
    }

    public void UpdateName(string newName, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("O nome da unidade não pode ser nulo ou vazio.", nameof(newName));
        }

        var normalized = newName.Trim();
        if (string.Equals(Name, normalized, StringComparison.Ordinal))
        {
            return;
        }

        Name = normalized;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete(Guid deletedBy)
    {
        if (DeletedAt.HasValue)
        {
            return;
        }

        DeletedAt = DateTime.UtcNow;
        UpdatedBy = deletedBy;
        UpdatedAt = DeletedAt.Value;
    }
}
