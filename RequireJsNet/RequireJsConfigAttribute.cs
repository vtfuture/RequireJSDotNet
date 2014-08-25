/*
 * RequireJS.NET
 * Copyright Stefan Prodan
 *   http://stefanprodan.eu
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 */

using System.Web.Mvc;

namespace RequireJsNet
{
    public class RequireJsConfigAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
            {
                var ctrl = (RequireJsController)filterContext.Controller;
                
                // these options are set in the PublicController code
                // options used in ~/Scripts/app-global.js
                ctrl.RegisterGlobalOptions();
                ctrl.RequireJsOptions.Save(RequireJsOptionsScope.Global);
            }

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
            {
                var ctrl = (RequireJsController)filterContext.Controller;
                
                // these options are set in the Action code
                // options used in  ~/Scripts/Controllers/Area/Controller/Controller-Action.js
                ctrl.RequireJsOptions.Save(RequireJsOptionsScope.Page);
            }

            base.OnActionExecuted(filterContext);
        }
    }
}