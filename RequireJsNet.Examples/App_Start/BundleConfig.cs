using System.Web;
using System.Web.Optimization;
using RequireJsNet.Compressor;

namespace RequireJsNet.Examples
{
	public class BundleConfig
	{
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new StyleBundle("~/Content/css").Include(
					  "~/Content/bootstrap.css",
					  "~/Content/site.css"));

			// Set EnableOptimizations to false for debugging. For more information,
			// visit http://go.microsoft.com/fwlink/?LinkId=301862
			BundleTable.EnableOptimizations = true;

			var opt = new RequireWebOptimization("~/Scripts/");
				

			foreach (var bundle in opt.CreateBundles())
			{
				bundles.Add(bundle);
			}
		}
	}
}
