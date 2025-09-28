using Chronosystem.Application.Features.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Units.Queries.GetAllUnits;

public record GetAllUnitsQuery() : IRequest<IEnumerable<UnitDto>>;