// src/PalletManagementSystem.Web/Global.asax.cs
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web2.Optimization;
using System.Web2.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using PalletManagementSystem.Web.App_Start;

namespace PalletManagementSystem.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Set up dependency injection
            var container = DependencyConfig.RegisterDependencies();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}