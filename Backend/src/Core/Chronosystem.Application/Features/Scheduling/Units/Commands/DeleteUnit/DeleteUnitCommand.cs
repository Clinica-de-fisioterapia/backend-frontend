using System;
using MediatR;

namespace Chronosystem.Application.Features.Scheduling.Units.Commands.DeleteUnit;

public sealed record DeleteUnitCommand(Guid UnitId, Guid UserId) : IRequest;

