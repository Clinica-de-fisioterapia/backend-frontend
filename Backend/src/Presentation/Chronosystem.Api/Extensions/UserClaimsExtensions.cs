using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Chronosystem.Application.Resources;

namespace Chronosystem.Api.Extensions
{
    public static class UserClaimsExtensions
    {
        public static Guid GetActorUserIdOrThrow(this ClaimsPrincipal user)
        {
            if (user == null)
                throw new UnauthorizedAccessException(Messages.Auth_InvalidToken_UserNotInContext);

            // 1) padrão JWT "sub"  2) fallback ASP.NET (NameIdentifier)  3) fallback "user_id"
            var userId =
                user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                user.FindFirstValue("user_id");

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var actorId))
                throw new UnauthorizedAccessException(Messages.Auth_InvalidToken_SubClaimMissingOrMalformed);

            return actorId;
        }

        public static string? GetTenantOrNull(this ClaimsPrincipal user)
        {
            return user?.FindFirstValue("tenant");
        }
    }
}
