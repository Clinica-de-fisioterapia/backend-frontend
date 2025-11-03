using System;
using System.Text.Json.Serialization;
using MediatR;

namespace Chronosystem.Application.Features.Professionals.Commands.DeleteProfessional;

public sealed record DeleteProfessionalCommand(Guid Id) : IRequest
{
    [JsonIgnore]
    public Guid ActorUserId { get; set; }
}
