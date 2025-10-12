using System;

namespace Chronosystem.Application.Features.Scheduling.Units.DTOs;

public record UnitDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt
);