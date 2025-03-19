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
                "~/Scripts/popper.js",
                "~/Scripts/bootstrap.js"
            ));

            // Modernizr bundle for feature detection
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"
            ));

            // Custom application scripts
            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                "~/Scripts/app/core.js",
                "~/Scripts/app/pallet.js",
                "~/Scripts/app/search.js",
                "~/Scripts/app/validation.js"
            ));

            // CSS bundles
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/font-awesome.css",
                "~/Content/pallet.css",
                "~/Content/site.css"
            ));

            // IE11 specific polyfills and compatibility fixes
            bundles.Add(new ScriptBundle("~/bundles/ie11compat").Include(
                "~/Scripts/ie11/polyfill.js",
                "~/Scripts/ie11/ie11-custom-properties.js"
            ));
        }
    }
}