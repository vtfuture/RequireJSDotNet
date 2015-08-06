using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace RequireJsNet.Examples
{
    public class MvcApplication : System.Web.HttpApplication
    {
	    private static bool _firstRequest = true;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

    }

		protected void Application_BeginRequest(object sender, EventArgs e)
		{
			if (_firstRequest)
			{
				var url = new UrlHelper(HttpContext.Current.Request.RequestContext);

				RequireJsOptions.Add("globalUrl", url.Action("Index", "Home"), RequireJsOptionsScope.Global);
				RequireJsOptions.Add("testDateTime", DateTime.Now, RequireJsOptionsScope.Global);

				_firstRequest = false;
			}
		}
    }
}
