using MediatR;

namespace Chronosystem.Application.Features.Units.Commands.UpdateUnit;

public record UpdateUnitCommand(Guid Id, string Name) : IRequest<Unit>;
