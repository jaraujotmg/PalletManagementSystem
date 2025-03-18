using System.Web.Optimization;

namespace PalletManagementSystem.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Disable optimization in debug mode
            BundleTable.EnableOptimizations = false;

            // jQuery bundle
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery.validate.js",
                "~/Scripts/jquery.validate.unobtrusive.js"
            ));

            // Bootstrap bundle
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/popper.js"
            ));

            // Modernizr bundle for feature detection
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"
            ));

            // CSS bundles
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/font-awesome.css",
                "~/Content/site.css"
            ));

            // Custom application scripts
            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                "~/Scripts/app/core.js",
                "~/Scripts/app/search.js",
                "~/Scripts/app/validation.js"
            ));
        }
    }
}