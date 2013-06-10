/*
 * RequireJS for .NET
 * Version 1.0.0.1
 * Release Date 06/09/0212
 * Copyright Stefan Prodan
 *   http://stefanprodan.eu
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RequireJS
{
    [RequireJsConfig]
    public abstract class RequireJsController : Controller
    {
        public RequireJsOptions RequireJsOptions;

        protected RequireJsController()
        {
            RequireJsOptions = new RequireJsOptions(this);
        }

        public abstract void RegisterGlobalOptions();
    }
}
