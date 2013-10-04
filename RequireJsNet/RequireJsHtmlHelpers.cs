/*
 * RequireJS.NET
 * Copyright Stefan Prodan
 *   http://stefanprodan.eu
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 */
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System;

namespace RequireJS
{
    public static class RequireJsHtmlHelpers
    {
        public static MvcHtmlString RenderRequireJsSetup(this HtmlHelper html, string baseUrl, string requireUrl)
        {
            var setupHtml = new StringBuilder();

            var entryPointPath = html.RequireJsEntryPoint();

            if(entryPointPath != null)
            {
                setupHtml.AppendLine("<script type=\"text/javascript\">");
                
                setupHtml.AppendLine("var requireConfig = {");
                setupHtml.Append("pageOptions:" + RequireJsOptions.ConvertToJsObject(html.ViewBag.PageOptions));
                setupHtml.AppendLine(",");
                setupHtml.AppendLine("websiteOptions:" + RequireJsOptions.ConvertToJsObject(html.ViewBag.GlobalOptions));
                setupHtml.AppendLine("};");

                setupHtml.AppendLine("var require = {");
                setupHtml.Append("locale:'" + html.CurrentCulture() + "'");
                setupHtml.AppendLine(",");
                setupHtml.Append("baseUrl:'" + baseUrl + "'");
                setupHtml.AppendLine(",");
                setupHtml.Append("paths:" + html.GetRequireJsPaths());
                setupHtml.AppendLine(",");
                setupHtml.AppendLine("shim:" + html.GetRequireJsShim());
                setupHtml.AppendLine("};");

                setupHtml.AppendLine("</script>");

                setupHtml.AppendLine("<script data-main=\"" + entryPointPath + "\" src=\"" + requireUrl + "\">");
                setupHtml.AppendLine("</script>");
            }

            return new MvcHtmlString(setupHtml.ToString());
        }

        public static MvcHtmlString RequireJsEntryPoint(this HtmlHelper html)
        {
            var area = html.ViewContext.RouteData.DataTokens["area"] != null
                       ? html.ViewContext.RouteData.DataTokens["area"].ToString()
                       : "Root";
            var controller = html.ViewContext.Controller.ValueProvider.GetValue("controller").RawValue as string;
            var action = html.ViewContext.Controller.ValueProvider.GetValue("action").RawValue as string;
			var entryPointTmpl = "Controllers/{0}/" + controller + "/" + controller + "-" + action;
            var entryPoint = string.Format(entryPointTmpl, area);
            var filePath = html.ViewContext.HttpContext.Server.MapPath("~/Scripts/" + entryPoint + ".js");
			if (File.Exists(filePath))
            {
                return new MvcHtmlString(entryPoint);
            }
            entryPoint = string.Format(entryPointTmpl, "Common");
            var fallbackFile = html.ViewContext.HttpContext.Server.MapPath("~/Scripts/" + entryPoint + ".js");
            return File.Exists(fallbackFile) ? new MvcHtmlString(entryPoint) : null;
        }

        public static MvcHtmlString GetRequireJsPaths(this HtmlHelper html)
        {
            var cfg = html.ViewContext.HttpContext.Server.MapPath("~/RequireJS.config");

            if (!File.Exists(cfg))
            {
                throw new FileNotFoundException("RequireJS config not found", cfg);
            }

            var result = new StringBuilder();
            var paths = XDocument.Load(cfg).Descendants("paths").Descendants("path");

#if !DEBUG
            
            var cfgRelease = html.ViewContext.HttpContext.Server.MapPath("~/RequireJS.Release.config");
            if (File.Exists(cfgRelease))
            {
                paths = XDocument.Load(cfgRelease).Descendants("paths").Descendants("path");
            }
#endif

            result.Append("{");
            foreach (var item in paths)
            {
                result.AppendFormat("\"{0}\":\"{1}\"{2}", item.Attribute("key").Value.Trim(), item.Attribute("value").Value.Trim(), paths.Last() == item ? "" : ",");
            }
            result.Append("}");

            return new MvcHtmlString(result.ToString());
        }

        public static MvcHtmlString GetRequireJsShim(this HtmlHelper html)
        {
            var cfg = html.ViewContext.HttpContext.Server.MapPath("~/RequireJS.config");

            if (!File.Exists(cfg))
            {
                throw new FileNotFoundException("RequireJS config not found", cfg);
            }

            var result = new StringBuilder();
            var shims = XDocument.Load(cfg).Descendants("shim").Descendants("dependencies");
#if !DEBUG
            
            var cfgRelease = html.ViewContext.HttpContext.Server.MapPath("~/RequireJS.Release.config");
            if (File.Exists(cfgRelease))
            {
                shims = XDocument.Load(cfg).Descendants("shim").Descendants("dependencies");
            }
#endif
            result.Append("{");
            foreach (var item in shims)
            {
                result.AppendFormat(" \"{0}\": {1} deps: [", item.Attribute("for").Value.Trim(), "{");
                var deps = item.Descendants("add");
                foreach (var dep in deps)
                {
                    result.AppendFormat("\"{0}\"{1}", dep.Attribute("dependency").Value.Trim(), deps.Last() == dep ? "" : ",");
                }

                var exports = item.Attribute("exports") != null && !string.IsNullOrEmpty(item.Attribute("exports").Value)
                                  ? ", exports: '" + item.Attribute("exports").Value.Trim() + "'"
                                  : string.Empty;

                result.AppendFormat("]{0} {1}{2} ", exports, "}", shims.Last() == item ? "" : ",");
            }
            result.Append("}");

            return new MvcHtmlString(result.ToString());
        }

        public static string CurrentCulture(this HtmlHelper html)
        {
            // split the ro-Ro string by '-' so it returns eg. ro / en
            return System.Threading.Thread.CurrentThread.CurrentCulture.Name.Split('-')[0];
        }

        public static Dictionary<string, int> ToJsonDictionary<TEnum>()
        {
            var enumType = typeof(TEnum);
            var names = Enum.GetNames(enumType);
            return Enum.GetNames(enumType).ToDictionary(r => r, r => Convert.ToInt32(Enum.Parse(enumType, r)));
        }
    }
}