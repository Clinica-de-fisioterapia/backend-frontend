using MediatR;
using System;

namespace Chronosystem.Application.Features.People.Commands.UpdatePerson
{
    public record UpdatePersonCommand(
        Guid Id,
        string FullName,
        string? Cpf,
        string? Phone,
        string? Email
    ) : IRequest
    {
        public Guid ActorUserId { get; set; }
    }
}
