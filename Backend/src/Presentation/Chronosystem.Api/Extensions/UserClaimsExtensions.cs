using System;
using System.Security.Claims;

namespace Chronosystem.Api.Extensions;

public static class UserClaimsExtensions
{
    public static Guid GetActorUserIdOrThrow(this ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var actorId))
            throw new UnauthorizedAccessException("Token inválido: claim 'sub' ausente ou malformada.");

        return actorId;
    }
}
