/*
 * RequireJS for .NET
 * Version 1.0.2.0
 * Release Date 26/08/2013
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
        public static MvcHtmlString RequireJsEntryPoint(this HtmlHelper html)
        {
            var area = html.ViewContext.RouteData.DataTokens["area"] != null
                       ? html.ViewContext.RouteData.DataTokens["area"].ToString()
                       : "Root";
            var controller = html.ViewContext.Controller.ValueProvider.GetValue("controller").RawValue as string;
            var action = html.ViewContext.Controller.ValueProvider.GetValue("action").RawValue as string;

            var entryPoint = "Controllers/" + area + "/" + controller + "/" + controller + "-" + action;
            var filePath = html.ViewContext.HttpContext.Server.MapPath("~/Scripts/" + entryPoint + ".js");

            return File.Exists(filePath) ? new MvcHtmlString(entryPoint) : null;
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
                paths = XDocument.Load(html.ViewContext.HttpContext.Server.MapPath("~/RequireJS.Release.config")).Descendants("paths").Descendants("path");
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
            var result = new StringBuilder();
            var shims = XDocument.Load(html.ViewContext.HttpContext.Server.MapPath("~/RequireJS.config")).Descendants("shim").Descendants("dependencies");

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