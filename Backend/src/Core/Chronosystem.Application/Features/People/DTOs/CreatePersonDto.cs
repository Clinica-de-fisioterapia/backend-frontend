namespace Chronosystem.Application.Features.People.DTOs
{
    public class CreatePersonDto
    {
        public string FullName { get; set; } = default!;
        public string? Cpf { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}
