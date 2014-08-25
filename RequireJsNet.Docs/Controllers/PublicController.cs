using RequireJS;
using RequireJsNet.Docs.Helpers;

namespace RequireJsNet.Docs.Controllers
{
    public abstract class PublicController : RequireJsController
    {
        public override void RegisterGlobalOptions()
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
