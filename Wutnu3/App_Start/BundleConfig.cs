using System.Web;
using System.Web.Optimization;

namespace Wutnu
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/scripts/jquery-{version}.js",
                        "~/scripts/jquery.gritter.js",
                        "~/scripts/lib/moment.min.js",
                        "~/scripts/lib/moment-timezone.min.js",
                        "~/scripts/lib/moment-tzData.js",
                        "~/scripts/lib/jstz-1.0.4.min.js",
                        "~/scripts/global.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/scripts/bootstrap.js",
                      "~/scripts/bootstrap-select.js",
                      "~/scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/manageportal").Include(
                      "~/scripts/app/manage/Manage.js",
                      "~/scripts/app/manage/_files.js",
                      "~/scripts/app/manage/_users.js",
                      "~/scripts/app/manage/_reports.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                    "~/scripts/lib/jquery.dataTables.js",
                    "~/scripts/lib/dataTables.bootstrap.js"));

            bundles.Add(new StyleBundle("~/content/datatablescss").Include(
                    "~/content/datatables/css/dataTables.bootstrap.css"));

            bundles.Add(new StyleBundle("~/content/css").Include(
                      "~/content/bootstrap.css",
                      "~/content/bootstrap-theme.css",
                      "~/content/bootstrap-select.css",
                      "~/content/jquery.gritter.css",
                      "~/content/site.css"));
        }
    }
}
