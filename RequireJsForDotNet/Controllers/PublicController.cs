using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RequireJS;

namespace RequireJsForDotNet.Controllers
{
    public abstract class PublicController : RequireJsController
    {
        public override void RegisterGlobalOptions()
        {
            RequireJsOptions.Add(
                "homeUrl",
                Url.Action("Index", "Home", new { area = "" }),
                RequireJsOptionsScope.Website
                );
            RequireJsOptions.Add(
                "adminUrl",
                Url.Action("Index", "Dashboard", new { area = "Admin" }),
                RequireJsOptionsScope.Website
                );
        }
    }
}
