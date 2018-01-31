using System.Web;
using System.Web.Optimization;

namespace weighingControlSystem
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
       
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Content/Theams/plugins/jquery/jquery.js",
                        "~/Content/Theams/plugins/bootstrap/js/bootstrap.js",
                        "~/Content/Theams/plugins/bootstrap-select/js/bootstrap-select.js",
                         "~/Content/Theams/plugins/jquery-slimscroll/jquery.slimscroll.js",
                         "~/Content/Theams/plugins/node-waves/waves.js",
                         "~/Content/Theams/plugins/jquery-countto/jquery.countTo.js",
                         "~/Content/Theams/plugins/bootstrap-select/js/bootstrap-select.js",
                         "~/Content/Theams/plugins/morrisjs/morris.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/Theams/css/style.css",
                                       "~/Content/Theams/plugins/bootstrap/css/bootstrap.css",
                                       "~/Content/Theams/plugins/node-waves/waves.css",
                                       "~/Content/Theams/plugins/animate-css/animate.css",
                                       "~/Content/Theams/plugins/morrisjs/morris.css",
                                       "~/Content/Theams/css/style.css",
                                       "~/Content/Theams/css/themes/all-themes.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));
        }
    }
}