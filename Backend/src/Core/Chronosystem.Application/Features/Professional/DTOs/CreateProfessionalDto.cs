using System;

namespace Chronosystem.Application.Features.Professionals.DTOs
{
    public class CreateProfessionalDto
    {
        public Guid PersonId { get; set; }
        public string? Specialty { get; set; }
    }
}
