using System;

namespace Chronosystem.Application.Features.People.DTOs
{
    public class PersonDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = default!;
        public string? Cpf { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long RowVersion { get; set; }
    }
}
