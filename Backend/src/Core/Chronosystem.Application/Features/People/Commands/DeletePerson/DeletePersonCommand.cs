using MediatR;
using System;

namespace Chronosystem.Application.Features.People.Commands.DeletePerson
{
    public class DeletePersonCommand : IRequest
    {
        public Guid Id { get; }
        public Guid ActorUserId { get; set; }

        public DeletePersonCommand(Guid id)
        {
            Id = id;
        }
    }
}
