using System;

namespace Chronosystem.Application.Features.Professionals.DTOs;

public sealed record ProfessionalResponseDto(
    Guid Id,
    Guid UserId,
    string? RegistryCode,
    string? Specialty,
    DateTime CreatedAt,
    Guid? CreatedBy,
    DateTime UpdatedAt,
    Guid? UpdatedBy,
    DateTime? DeletedAt,
    long RowVersion
);
