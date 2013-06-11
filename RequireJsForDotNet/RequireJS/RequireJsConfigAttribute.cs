/*
 * RequireJS for .NET
 * Version 1.0.0.1
 * Release Date 10/06/0213
 * Copyright Stefan Prodan
 *   http://stefanprodan.eu
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 */
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace RequireJS
{
    public class RequireJsConfigAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
            {
                var ctrl = (RequireJsController) filterContext.Controller;
                
                //these options are set in the PublicController code
                //options used in ~/Scripts/app-global.js
                ctrl.RegisterGlobalOptions();
                ctrl.RequireJsOptions.Save(RequireJsOptionsScope.Website);
            }
            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
            {
                var ctrl = (RequireJsController)filterContext.Controller;
                
                //these options are set in the Action code
                //options used in  ~/Scripts/Controllers/Area/Controller/Controller-Action.js
                ctrl.RequireJsOptions.Save(RequireJsOptionsScope.Page);
            }
            base.OnActionExecuted(filterContext);
        }
    }
}