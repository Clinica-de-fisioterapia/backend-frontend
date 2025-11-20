using System;

namespace Chronosystem.Domain.Entities
{
    public class Person
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = default!;
        public string? Cpf { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long RowVersion { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
