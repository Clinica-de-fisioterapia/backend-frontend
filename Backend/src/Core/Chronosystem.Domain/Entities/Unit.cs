// Chronosystem.Domain/Entities/Unit.cs

using Chronosystem.Domain.Shared; // Supondo um namespace para a classe base

namespace Chronosystem.Domain.Entities;

// Usaremos uma classe base para propriedades comuns, o que é uma ótima prática.
public class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class BaseEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
}

// Nossa entidade Unit herda as propriedades comuns
public class Unit : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}