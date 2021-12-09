using JankiTransfer.ChangeDetection;
using JankiWeb.Services;
using JankiWeb.StartupExtentions;
using JankiWebCards.Janki.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace JankiWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<JankiWebContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("JankiDatabase"));
                options.EnableSensitiveDataLogging();
            });

            services.AddJankiIdentity();

            services.AddScoped<IChangeContext<JankiWebContext>, JankiWebChangeContext>();
            services.AddScoped<ChangeDetector<JankiWebContext>>();
            services.AddScoped<IBundleManagerService, BundleManagerService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "JankiWeb", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, JankiWebContext context)
        {
            context.Database.EnsureCreated();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "JankiWeb v1"));
            }

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}