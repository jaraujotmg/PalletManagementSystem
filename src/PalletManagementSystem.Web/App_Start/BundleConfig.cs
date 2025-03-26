using System.Web;
using System.Web.Optimization;

namespace PalletManagementSystem.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/popper.js",
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/font-awesome.css",
                      "~/Content/site.css"));

            // Custom bundle for Pallet Management System
            bundles.Add(new ScriptBundle("~/bundles/pms").Include(
                      "~/Scripts/ie11-polyfills.js",
                      "~/Scripts/modal-helpers.js",
                      "~/Scripts/form-validation.js",
                      "~/Scripts/pallet-management.js"));

            // Add custom CSS for IE11 specific fixes
            bundles.Add(new StyleBundle("~/Content/ie11-fixes").Include(
                      "~/Content/ie11-fixes.css"));

            // In production, enable optimization
            BundleTable.EnableOptimizations = true;
        }
    }
}