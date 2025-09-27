using MediatR;
namespace Chronosystem.Application.Features.Units.Commands.UpdateUnit;
public record UpdateUnitCommand(Guid UnitId, string Name, Guid TenantId, Guid UserId) : IRequest;