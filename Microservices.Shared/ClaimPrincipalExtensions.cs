using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microservices.Shared
{
    public static class ClaimPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal user) =>
       user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public static string? GetUserRole(this ClaimsPrincipal user) =>
            user?.FindFirst(ClaimTypes.Role)?.Value;
    }
}