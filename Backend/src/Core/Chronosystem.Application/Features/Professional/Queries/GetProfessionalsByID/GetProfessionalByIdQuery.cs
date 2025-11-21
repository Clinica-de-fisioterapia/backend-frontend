using Chronosystem.Application.Features.Professionals.DTOs;
using MediatR;
using System;

namespace Chronosystem.Application.Features.Professionals.Queries.GetProfessionalById
{
    public record GetProfessionalByIdQuery(Guid Id) : IRequest<ProfessionalDto>;
}
