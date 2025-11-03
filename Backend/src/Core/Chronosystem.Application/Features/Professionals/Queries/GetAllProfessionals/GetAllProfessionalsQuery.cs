using System;
using System.Collections.Generic;
using Chronosystem.Application.Features.Professionals.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Professionals.Queries.GetAllProfessionals;

public sealed record GetAllProfessionalsQuery(Guid? UserId, string? RegistryCode, string? Specialty) : IRequest<IEnumerable<ProfessionalResponseDto>>;
