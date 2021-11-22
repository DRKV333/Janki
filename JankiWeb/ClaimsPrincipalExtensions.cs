using JankiWeb.StartupExtentions;
using System;
using System.Linq;
using System.Security.Claims;

namespace JankiWeb
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetBundleId(this ClaimsPrincipal user)
            => Guid.Parse(user.Claims.First(x => x.Type.Equals(IdentityServerConfig.BundleIdClaim, StringComparison.OrdinalIgnoreCase))?.Value);
    }
}