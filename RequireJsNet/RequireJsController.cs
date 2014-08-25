// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Web.Mvc;

namespace RequireJsNet
{
    [RequireJsConfig]
    public abstract class RequireJsController : Controller
    {
        protected RequireJsController()
        {
            RequireJsOptions = new RequireJsOptions(this);
        }

        public RequireJsOptions RequireJsOptions { get; set; }

        public abstract void RegisterGlobalOptions();
    }
}
