using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace JankiWeb.StartupExtentions
{
    internal static class IdentityServerConfig
    {
        public const string JankiAPIName = "undersea";
        public const string JankiAPIDesc = "Undersea API";
        public const string ROPClientId = "rop";
        public const string ROPClientSecret = "SuperSecretClientSecret";

        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope(JankiAPIName, JankiAPIDesc, new string[] {
                    ClaimTypes.NameIdentifier,
                    JwtClaimTypes.Name,
                    ClaimTypes.Role
                })
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "rop",
                    ClientSecrets = { new Secret(ROPClientSecret.Sha256()) },
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowedScopes = { JankiAPIName, IdentityServerConstants.StandardScopes.OfflineAccess },
                    AllowOfflineAccess = true,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    AlwaysSendClientClaims = true
                }
            };
    }
}