using System;

namespace Chronosystem.Domain.Common
{
    /// <summary>Auditoria + soft delete (UTC). Implementa IAuditableEntity e ISoftDeleteEntity.</summary>
    public abstract class AuditableEntity : BaseEntity, IAuditableEntity, ISoftDeleteEntity
    {
        // Setters PÚBLICOS para satisfazer as interfaces.
        public DateTime CreatedAt  { get; set; }
        public Guid?    CreatedBy  { get; set; }
        public DateTime UpdatedAt  { get; set; }
        public Guid?    UpdatedBy  { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Helpers opcionais
        protected void MarkCreated(Guid? userId)
        {
            var now = DateTime.UtcNow;
            CreatedAt = now; UpdatedAt = now;
            CreatedBy = userId; UpdatedBy = userId;
        }

        protected void MarkUpdated(Guid? userId)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = userId;
        }

        public void SoftDelete(Guid? userId = null)
        {
            var now = DateTime.UtcNow;
            DeletedAt = now; UpdatedAt = now; UpdatedBy = userId;
        }
    }
}
