using System.Web;
using System.Web.Optimization;

namespace neverending
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                        "~/Scripts/jquery-2.0.3.min.js", "~/Scripts/jquery.timeago.js",
                        "~/content/site.js"));

            bundles.Add(new ScriptBundle("~/bundles/allscripts").Include(
            "~/Scripts/jquery-1.10.1min.js",
            "~/Scripts/jquery-ui-{version}.js",
            "~/Scripts/moment-with-langs.js",
            "~/Scripts/jquery.cookie.js",
            //"~/Scripts/jquery.lazyscrollloading-min.js",
            //"~/Scripts/jquery.slimscroll.js",
            "~/Scripts/jquery.jscrollmin.js",
            "~/Scripts/jquery.countdown.js",
            "~/Scripts/jquery.countdown-tr.js",
            "~/Scripts/jquery.textrange.js",
                "~/Scripts/moxie.min.js",
                "~/Scripts/plupload.min.js",
                //"~/Scripts/jquery.mousewheel.min.js",
                
            //"~/Scripts/plupload.full.min.js",
            "~/Content/site.js"
            ));

            bundles.Add(new StyleBundle("~/Content/css1").Include("~/Content/style.css"));
            bundles.Add(new StyleBundle("~/Content/css2").Include("~/Content/jquery-ui.1.10.3.css"));
            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/style.css", "~/Content/jquery-ui.1.10.3.css"));
        }
    }
}