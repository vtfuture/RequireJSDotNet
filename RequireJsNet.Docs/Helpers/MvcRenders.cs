using System.IO;
using System.Web.Mvc;

namespace RequireJsNet.Docs.Helpers
{
    public static class MvcRenders
    {
        public static string RenderPartialView(this ControllerBase controller, string viewName, object model, string htmlFieldPrefix = "")
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");
            var backup = controller.ViewData.Model;
            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);

                var backupPrefix = controller.ViewData.TemplateInfo.HtmlFieldPrefix;
                controller.ViewData.TemplateInfo.HtmlFieldPrefix = htmlFieldPrefix;

                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewContext.FormContext = null;
                viewResult.View.Render(viewContext, sw);

                controller.ViewData.TemplateInfo.HtmlFieldPrefix = backupPrefix;
                controller.ViewData.Model = backup;
                return sw.GetStringBuilder().ToString().Trim();
            }
        }

        public static string RenderView(this Controller controller, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindView(controller.ControllerContext, viewName, null);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString().Trim();
            }
        }

        public static string RenderAppVersion()
        {
            return "v=" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}