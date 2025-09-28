namespace Chronosystem.Domain.Common;

public abstract class AuditableEntity : BaseEntity, IAuditableEntity, ISoftDeleteEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}