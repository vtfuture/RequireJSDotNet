using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RequireJsForDotNet.Helpers
{
    public enum AjaxRequestStatus
    {
        Success = 1,
        ServerError = 2,
        Unauthorized = 3,
        InvalidData = 4,
        RedirectRequired = 5
    }
}