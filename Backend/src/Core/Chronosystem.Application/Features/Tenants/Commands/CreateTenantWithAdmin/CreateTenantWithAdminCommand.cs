using System;
using MediatR;

namespace Chronosystem.Application.Features.Tenants.Commands.CreateTenantWithAdmin;

public sealed record CreateTenantWithAdminCommand(
    string Slug,
    string Name,
    string AdminFullName,
    string AdminEmail,
    string AdminPassword
) : IRequest
{
    public Guid ActorUserId { get; init; }
}
