using Cuttr.Business.Exceptions;
using System.Security.Claims;

namespace Cuttr.Api.Common
{
    public static class GetInfoFromClaims
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            throw new Business.Exceptions.UnauthorizedAccessException("Invalid token: User ID Claim is not valid.");
        }
    }
}
