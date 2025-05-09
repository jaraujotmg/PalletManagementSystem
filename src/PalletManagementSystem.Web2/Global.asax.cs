// src/PalletManagementSystem.Web2/Global.asax.cs
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using PalletManagementSystem.Web2.App_Start;

namespace PalletManagementSystem.Web2
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            try
            {

           
           
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Set up dependency injection
            var container = DependencyConfig.RegisterDependencies();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            }
            catch (Exception ex)
            {

                // Log detailed exception
                System.Diagnostics.Debug.WriteLine($"Startup Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Optionally, you could write to a log file
                System.IO.File.WriteAllText("startup_error_log.txt",
                    $"Startup Error: {ex.Message}\n{ex.StackTrace}");

                throw; // Re-throw to ensure startup failure is visible
            }
        }
    }
}