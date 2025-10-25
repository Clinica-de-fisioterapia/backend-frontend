using System;

namespace Chronosystem.Domain.Common
{
    /// <summary>Base enxuta: apenas identidade (e opcionalmente concurrency).</summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; }

        // Concurrency/ETag opcional; mantenha se existir coluna no banco
        public long RowVersion { get; set; }
    }
}
