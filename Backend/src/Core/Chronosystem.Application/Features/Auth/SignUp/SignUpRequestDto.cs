using System;

namespace Chronosystem.Application.Features.Auth.SignUp;

/// <summary>
/// Input DTO for self-service tenant sign-up.
/// </summary>
public sealed class SignUpRequestDto
{
    public string CompanyName { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty; // e.g., clinica-sol
    public string AdminFullName { get; init; } = string.Empty;
    public string AdminEmail { get; init; } = string.Empty;
    public string AdminPassword { get; init; } = string.Empty;
}
