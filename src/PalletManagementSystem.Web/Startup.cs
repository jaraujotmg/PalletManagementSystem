// src/PalletManagementSystem.Web/Startup.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PalletManagementSystem.Web
{
    // NOTE: This file is not typically used in a traditional ASP.NET MVC 5 application running on .NET Framework 4.8.
    // It's included here for compatibility with modern tooling and to support potential future migration to ASP.NET Core.
    // The actual configuration and dependency injection setup is in Global.asax.cs and App_Start files.
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Note: Service configuration for ASP.NET MVC 5 is done using Autofac in DependencyConfig.cs
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Note: Pipeline configuration for ASP.NET MVC 5 is done in Global.asax.cs and Web.config
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}