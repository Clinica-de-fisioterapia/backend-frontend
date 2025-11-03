namespace Chronosystem.Application.Features.Professionals.DTOs;

public sealed record UpdateProfessionalDto(
    string? RegistryCode,
    string? Specialty
);
