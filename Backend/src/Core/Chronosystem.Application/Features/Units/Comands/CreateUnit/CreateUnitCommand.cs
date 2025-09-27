// Features/Units/Commands/CreateUnit/CreateUnitCommand.cs
using Chronosystem.Application.Features.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Units.Commands.CreateUnit;

public record CreateUnitCommand(string Name, Guid TenantId, Guid UserId) : IRequest<UnitDto>;