using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RequireJsNet.Examples.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            RequireJsOptions.Add("globalUrlViaInheritance", Url.Action("Index", "Home"), RequireJsOptionsScope.Global);
        }
    }
}