using System.Web.Optimization;

namespace RequireJsNet.Docs.App_Start
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/css")
                .Include("~/Content/*.css", new CssRewriteUrlTransform())
                .Include("~/Scripts/Lib/Vendor/jQuery/UI/themes/base/jquery-ui.css", new CssRewriteUrlTransform())
                );
        }
    }
}