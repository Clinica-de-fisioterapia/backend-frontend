// Core/Chronosystem.Domain/Common/BaseEntity.cs
namespace Chronosystem.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }

    // (opcional) campos de auditoria/concurrency — mantenha só se você usa
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public long RowVersion { get; set; }
}
