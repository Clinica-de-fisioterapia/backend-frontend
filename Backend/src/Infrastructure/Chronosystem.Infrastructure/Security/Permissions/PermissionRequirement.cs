using System;
using Microsoft.AspNetCore.Authorization;

namespace Chronosystem.Infrastructure.Security.Permissions;

/// <summary>
/// Authorization requirement that verifies if the current user owns a specific permission claim.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }

    public string Permission { get; }
}
