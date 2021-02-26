using System.Web.Optimization;

namespace ead_frontend
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/umd/popper.js", 
                "~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/mark").Include("~/Scripts/jquery.mark.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/chart").Include("~/Scripts/Chart.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/fontawesome")
                .Include("~/Content/font-awesome.css"));

            bundles.Add(new ScriptBundle("~/bundles/datetimepicker").Include(
                "~/Scripts/moment.min.js",
                "~/Scripts/tempusdominus-bootstrap-4.min.js"));

            bundles.Add(new StyleBundle("~/Content/datetimepicker").Include("~/Content/tempusdominus-bootstrap-4.min.css"));

        }
    }
}
