using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using RequireJS;
using RequireJsForDotNet.Helpers;

namespace RequireJsForDotNet.Controllers
{
    public class KeepaliveController : PublicController
    {
        /// <summary>
        /// Short-polling
        /// </summary>
        public JsonResult Ping()
        {
            var status = AjaxRequestStatus.Success;
            var messages = 0;
            var error = string.Empty;

            //TODO: redirect to login
            var redirectUrl = Url.Action("Inbox", "Home");

            try
            {
                //TODO: check user cookie, session, IsAuthorized
                if (Session["LastPing"] == null)
                {
                    status = AjaxRequestStatus.Unauthorized;
                    error = "Unauthorized access detected";

                    //unauthorized fix, triggers page refresh
                    Session["LastPing"] = DateTime.Now.ToBinary();
                }
                else
                {
                    //get last access date
                    var lastPing = DateTime.FromBinary(Convert.ToInt64(Session["LastPing"]));
#if DEBUG
                    //throw new Exception("simulate server crash");

                    //throw warning if delay is over 4ms
                    var totalMs = Convert.ToInt32((DateTime.Now - lastPing).TotalMilliseconds);
                    if (totalMs > 4000)
                    {
                        error = string.Format("Last ping was {0} milisec ago", totalMs);
                    }
#endif
                    //TODO: count new messages in cache, db or web service
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    messages = new Random().Next(0, 9);

                    //update last access date
                    Session["LastPing"] = DateTime.Now.ToBinary();
                }
            }
            catch (Exception ex)
            {
                status = AjaxRequestStatus.ServerError;
                error = ex.Message;
            }

            return Json(new
            {
                Status = (int) status,
                Messages = messages,
                Error = error,
                RedirectUrl = redirectUrl
            }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Long-polling
        /// Suitable for long running db queries or web service calls
        /// </summary>
        public async Task<ActionResult> PingAsync()
        {
            await Task.Yield();

            return Ping();
        }

    }
}
