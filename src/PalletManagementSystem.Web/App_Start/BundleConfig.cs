// src/PalletManagementSystem.Web/App_Start/BundleConfig.cs
using System.Web.Optimization;

namespace PalletManagementSystem.Web.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleTable bundles)
        {
            // Bootstrap bundles
            bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                      "~/Content/bootstrap.min.css"));

            bundles.Add(new StyleBundle("~/Content/fontawesome").Include(
                      "~/Content/all.min.css"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                      "~/Scripts/jquery-{version}.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/popper.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                      "~/Scripts/jquery.validate.min.js",
                      "~/Scripts/jquery.validate.unobtrusive.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                      "~/Scripts/site.js"));

            // IE compatibility settings
            BundleTable.EnableOptimizations = true;
        }
    }
}