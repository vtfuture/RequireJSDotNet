using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RequireJsNet.Examples.Attributes
{
    public class RequireOptionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var url = new UrlHelper(filterContext.RequestContext);
            RequireJsOptions.Add("globalUrlViaFilter", url.Action("Index", "Home"), RequireJsOptionsScope.Global);
        }
    }
}