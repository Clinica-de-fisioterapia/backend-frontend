using MediatR;
using System;

namespace Chronosystem.Application.Features.People.Commands.CreatePerson
{
    public record CreatePersonCommand(
        string FullName,
        string? Cpf,
        string? Phone,
        string? Email
    ) : IRequest<Guid>
    {
        public Guid ActorUserId { get; set; }
    }
}
