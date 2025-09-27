// Chronosystem.Application/Features/Units/DTOs/UnitDto.cs
namespace Chronosystem.Application.Features.Units.DTOs;

public record UnitDto(
    Guid Id,
    Guid TenantId,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt
);