using System.Web;
using System.Web.Mvc;

using RequireJsNet.Examples.Attributes;

namespace RequireJsNet.Examples
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new RequireOptionFilter());
        }
    }
}
