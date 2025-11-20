using MediatR;
using System;

namespace Chronosystem.Application.Features.People.Commands.CreatePerson
{
        public class CreatePersonCommand : IRequest<Guid>
    {
    public CreatePersonCommand(string fullName, string? cpf, string? phone, string? email)
    {
      FullName = fullName;
      Cpf = cpf;
      Phone = phone;
      Email = email;
    }

    public string FullName { get; set; }
        public string? Cpf { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
    
}
