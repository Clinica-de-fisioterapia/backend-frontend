using System;
using System.Text.Json.Serialization;
using Chronosystem.Application.Features.Professionals.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Professionals.Commands.UpdateProfessional;

public sealed record UpdateProfessionalCommand(Guid Id, string? RegistryCode, string? Specialty) : IRequest<ProfessionalResponseDto>
{
    [JsonIgnore]
    public Guid ActorUserId { get; set; }
}
