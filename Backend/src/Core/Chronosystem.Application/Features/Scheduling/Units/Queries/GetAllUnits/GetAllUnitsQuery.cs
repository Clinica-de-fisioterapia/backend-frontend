using System.Collections.Generic;
using Chronosystem.Application.Features.Scheduling.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Scheduling.Units.Queries.GetAllUnits;

public sealed record GetAllUnitsQuery() : IRequest<IEnumerable<UnitDto>>;