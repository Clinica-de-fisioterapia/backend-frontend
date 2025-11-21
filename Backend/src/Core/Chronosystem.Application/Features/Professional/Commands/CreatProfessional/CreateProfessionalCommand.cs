using Chronosystem.Application.Features.Professionals.DTOs;
using MediatR;
using System;

namespace Chronosystem.Application.Features.Professionals.Commands.CreateProfessional
{
    public record CreateProfessionalCommand(CreateProfessionalDto Dto) : IRequest<Guid>;
}
