using System;
using MediatR;

namespace Chronosystem.Application.Features.Units.Commands.UpdateUnit;

public sealed record UpdateUnitCommand(Guid UnitId, string Name, Guid UserId) : IRequest;
