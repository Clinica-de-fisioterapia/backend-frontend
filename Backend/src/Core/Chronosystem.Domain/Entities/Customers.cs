using System;

namespace Chronosystem.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }
        public Guid PersonId { get; set; }

        public Person? Person { get; set; }

        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public long RowVersion { get; set; }
    }
}
