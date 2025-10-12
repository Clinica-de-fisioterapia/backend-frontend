using System;
using Chronosystem.Application.Features.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Units.Queries.GetUnitById;

public sealed record GetUnitByIdQuery(Guid UnitId) : IRequest<UnitDto?>;
