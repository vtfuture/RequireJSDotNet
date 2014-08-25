using System.Web.Mvc;

using RequireJsDotNet.Models;

namespace RequireJsDotNet.Helpers
{
    internal static class HtmlHelpers
    {
        public static RoutingInfo GetRoutingInfo(this HtmlHelper html)
        {
            var area = html.ViewContext.RouteData.DataTokens["area"] != null
                ? html.ViewContext.RouteData.DataTokens["area"].ToString()
                : "Root";

            var controller = html.ViewContext.Controller.ValueProvider.GetValue("controller").RawValue as string;
            var action = html.ViewContext.Controller.ValueProvider.GetValue("action").RawValue as string;
            return new RoutingInfo
            {
                Area = area,
                Controller = controller,
                Action = action
            };
        }
    }
}
