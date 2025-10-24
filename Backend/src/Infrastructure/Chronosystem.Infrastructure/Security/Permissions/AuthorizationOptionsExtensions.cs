using Microsoft.AspNetCore.Authorization;

namespace Chronosystem.Infrastructure.Security.Permissions;

public static class AuthorizationOptionsExtensions
{
    public static AuthorizationOptions AddPermissionPolicy(
        this AuthorizationOptions options,
        string policyName,
        string permission)
    {
        options.AddPolicy(policyName, policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));

        return options;
    }
}
