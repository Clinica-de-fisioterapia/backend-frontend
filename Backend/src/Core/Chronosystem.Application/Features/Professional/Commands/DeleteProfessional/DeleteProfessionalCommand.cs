using MediatR;
using System;

namespace Chronosystem.Application.Features.Professionals.Commands.DeleteProfessional
{
    public record DeleteProfessionalCommand(Guid Id) : IRequest;
}
