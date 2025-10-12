using System.Collections.Generic;
using Chronosystem.Application.Features.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Units.Queries.GetAllUnits;

public sealed record GetAllUnitsQuery() : IRequest<IEnumerable<UnitDto>>;
