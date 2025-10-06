
using Chronosystem.Application.Features.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Units.Commands.CreateUnit;


public record CreateUnitCommand(string Name, Guid UserId) : IRequest<UnitDto>;