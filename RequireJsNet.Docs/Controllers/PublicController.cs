using System.Web.Mvc;

using RequireJsNet;
using RequireJsNet.Docs.Helpers;

namespace RequireJsNet.Docs.Controllers
{
    public abstract class PublicController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            RequireJsOptions.Add(
                "homeUrl",
                Url.Action("Index", "Home", new { area = "" }),
                RequireJsOptionsScope.Global);

            RequireJsOptions.Add(
                "adminUrl",
                Url.Action("Index", "Dashboard", new { area = "Admin" }),
                RequireJsOptionsScope.Global);
            
            RequireJsOptions.Add(
                "statusEnum",
                RequireJsHtmlHelpers.ToJsonDictionary<AjaxRequestStatus>(),
                RequireJsOptionsScope.Global);
        }
    }
}
