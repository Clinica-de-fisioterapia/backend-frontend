using MediatR;

namespace Chronosystem.Application.Features.Units.Commands.DeleteUnit;

public record DeleteUnitCommand(Guid UnitId, Guid UserId) : IRequest;

