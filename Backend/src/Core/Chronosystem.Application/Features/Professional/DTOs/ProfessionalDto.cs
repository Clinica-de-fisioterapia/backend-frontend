using Chronosystem.Application.Features.People.DTOs;
using System;

namespace Chronosystem.Application.Features.Professionals.DTOs
{
    public class ProfessionalDto
    {
        public Guid Id { get; set; }
        public Guid PersonId { get; set; }
        public PersonDto Person { get; set; } = default!;
        public string? Specialty { get; set; }
    }
}
