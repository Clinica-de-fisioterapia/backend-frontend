using MediatR;
using System;

namespace Chronosystem.Application.Features.Auth.SignUp;

/// <summary>
/// Command to provision a new tenant schema and seed the first Admin user.
/// Returns the created company (tenant) id.
/// </summary>
public sealed record SignUpCommand(
    string CompanyName,
    string Subdomain,
    string AdminFullName,
    string AdminEmail,
    string AdminPassword
) : IRequest<Guid>;
