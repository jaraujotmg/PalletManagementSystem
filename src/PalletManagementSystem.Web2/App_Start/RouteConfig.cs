// src/PalletManagementSystem.Web/App_Start/RouteConfig.cs
using System.Web.Mvc;
using System.Web.Routing;

namespace PalletManagementSystem.Web2.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}