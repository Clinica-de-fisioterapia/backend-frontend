using Chronosystem.Application.Features.Professionals.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Chronosystem.Application.Features.Professionals.Queries.GetAllProfessionals
{
    public record GetAllProfessionalsQuery() : IRequest<IEnumerable<ProfessionalDto>>;
}
