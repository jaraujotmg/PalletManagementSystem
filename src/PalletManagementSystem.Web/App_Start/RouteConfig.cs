using System.Web.Mvc;
using System.Web.Routing;

namespace PalletManagementSystem.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Default route
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "PalletManagementSystem.Web.Controllers" }
            );

            // Pallets route
            routes.MapRoute(
                name: "Pallets",
                url: "Pallets/{action}/{id}",
                defaults: new { controller = "Pallets", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "PalletManagementSystem.Web.Controllers" }
            );

            // Items route
            routes.MapRoute(
                name: "Items",
                url: "Items/{action}/{id}",
                defaults: new { controller = "Items", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "PalletManagementSystem.Web.Controllers" }
            );

            // Search route
            routes.MapRoute(
                name: "Search",
                url: "Search/{action}",
                defaults: new { controller = "Search", action = "Index" },
                namespaces: new[] { "PalletManagementSystem.Web.Controllers" }
            );
        }
    }
}