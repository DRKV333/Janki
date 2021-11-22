using JankiWebCards.Janki;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JankiWeb.StartupExtentions
{
    public class JankiClaimsFactory : UserClaimsPrincipalFactory<JankiUser>
    {
        public JankiClaimsFactory(UserManager<JankiUser> userManager, IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(JankiUser user)
        {
            ClaimsIdentity claims = await base.GenerateClaimsAsync(user);
            claims.AddClaim(new Claim(IdentityServerConfig.BundleIdClaim, user.BundleId.ToString()));
            return claims;
        }
    }
}