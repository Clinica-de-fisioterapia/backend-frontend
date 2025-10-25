using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Chronosystem.Infrastructure.Security.Permissions;

/// <summary>
/// Evaluates <see cref="PermissionRequirement"/> against the permissions claim provided by the JWT token.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        var permissions = context.User.FindAll("permissions")
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value));

        if (permissions.Any(value => string.Equals(value, requirement.Permission, StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
