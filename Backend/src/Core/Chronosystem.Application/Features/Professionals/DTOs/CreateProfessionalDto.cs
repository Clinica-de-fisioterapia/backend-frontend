using System;

namespace Chronosystem.Application.Features.Professionals.DTOs;

public sealed record CreateProfessionalDto(
    Guid UserId,
    string? RegistryCode,
    string? Specialty
);
