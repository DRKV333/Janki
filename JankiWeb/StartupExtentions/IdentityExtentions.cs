using JankiWebCards.Janki;
using JankiWebCards.Janki.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace JankiWeb.StartupExtentions
{
    internal static class IdentityExtensions
    {
        public static void AddJankiIdentity(this IServiceCollection services)
        {
            services.AddIdentity<JankiUser, IdentityRole>()
                .AddEntityFrameworkStores<JankiWebContext>();

            services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.IssuerUri = "http://localhost:5000";
                options.EmitStaticAudienceClaim = true;
            })
                .AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
                .AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
                .AddInMemoryClients(IdentityServerConfig.Clients)
                .AddAspNetIdentity<JankiUser>()
                .AddDeveloperSigningCredential();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;

                    options.Authority = "http://localhost:5000";
                    options.Audience = "http://localhost:5000/resources";
                });
        }
    }
}