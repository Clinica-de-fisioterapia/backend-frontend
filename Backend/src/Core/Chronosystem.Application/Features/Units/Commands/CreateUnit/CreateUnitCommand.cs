using System;
using Chronosystem.Application.Features.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Units.Commands.CreateUnit;

public sealed record CreateUnitCommand(string Name, Guid UserId) : IRequest<UnitDto>;
