
using System;
using Chronosystem.Application.Features.Scheduling.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Scheduling.Units.Commands.CreateUnit;

public sealed record CreateUnitCommand(string Name, Guid UserId) : IRequest<UnitDto>;