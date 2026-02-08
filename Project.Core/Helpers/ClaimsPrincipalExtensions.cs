using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Project.Core.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? user.FindFirst("sub")?.Value
                       ?? user.FindFirst("id")?.Value
                       ?? user.FindFirst("uid")?.Value;

            if (Guid.TryParse(idClaim, out var userId))
            {
                return userId;
            }

            return Guid.Empty; // أو throw new UnauthorizedAccessException();
        }
    }
}
