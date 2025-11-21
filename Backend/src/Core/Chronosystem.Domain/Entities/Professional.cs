using Chronosystem.Domain.Common;
using System;

namespace Chronosystem.Domain.Entities
{
    public class Professional : AuditableEntity
    {
        public Guid PersonId { get; set; }
        public Person Person { get; set; } = default!;

        public string? Specialty { get; set; }

        public void UpdateSpecialty(string? specialty) => Specialty = specialty;
    }
}
