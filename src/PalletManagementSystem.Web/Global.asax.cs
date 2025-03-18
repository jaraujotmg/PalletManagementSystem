using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Optimization;
using Microsoft.Extensions.DependencyInjection;
using PalletManagementSystem.Infrastructure.Extensions;

namespace PalletManagementSystem.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Configure Dependency Injection
            ConfigureDependencyInjection();

            // Areas registration
            AreaRegistration.RegisterAllAreas();

            // Filter configuration
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            // Route configuration
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Bundle configuration
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private void ConfigureDependencyInjection()
        {
            // Create service collection
            var services = new ServiceCollection();

            // Configure infrastructure services using extension method
            services.AddInfrastructureServices(System.Configuration.ConfigurationManager.ConnectionStrings);

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Set MVC dependency resolver
            DependencyResolver.SetResolver(new ServiceProviderDependencyResolver(serviceProvider));
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Add CORS headers if needed
            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");
                HttpContext.Current.Response.End();
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            // Optional: Additional authentication logic
            if (Context.User.Identity.IsAuthenticated)
            {
                // Log authentication details or perform additional checks
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            Server.ClearError();

            // Log the error
            System.Diagnostics.Trace.WriteLine($"Unhandled Exception: {exception.Message}");
            System.Diagnostics.Trace.WriteLine($"Stack Trace: {exception.StackTrace}");

            // Redirect to error page
            Response.Redirect($"~/Error?message={HttpUtility.UrlEncode(exception.Message)}");
        }

        // Custom dependency resolver for .NET Framework
        public class ServiceProviderDependencyResolver : IDependencyResolver
        {
            private readonly IServiceProvider _serviceProvider;

            public ServiceProviderDependencyResolver(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public object GetService(Type serviceType)
            {
                return _serviceProvider.GetService(serviceType);
            }

            public System.Collections.Generic.IEnumerable<object> GetServices(Type serviceType)
            {
                return _serviceProvider.GetServices(serviceType);
            }
        }
    }
}