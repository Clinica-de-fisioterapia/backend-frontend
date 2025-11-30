using Chronosystem.Domain.Common;
using System;

namespace Chronosystem.Domain.Entities
{
    public class Booking : AuditableEntity, IAggregateRoot
    {
        public Guid ProfessionalId { get; set; }
        public Professional Professional { get; set; } = default!;

        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;

        public Guid ServiceId { get; set; }
        public Service Service { get; set; } = default!;

        public Guid UnitId { get; set; }
        public Unit Unit { get; set; } = default!;

        // ✅ Agora em DateTime (UTC)
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string Status { get; set; } = "confirmed";

        public void UpdateStatus(string status) => Status = status;
    }
}
