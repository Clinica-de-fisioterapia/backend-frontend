using System;
using Chronosystem.Application.Features.Professionals.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Professionals.Queries.GetProfessionalById;

public sealed record GetProfessionalByIdQuery(Guid Id) : IRequest<ProfessionalResponseDto?>;
